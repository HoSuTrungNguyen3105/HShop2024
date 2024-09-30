
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HShop2024.Data;
using HShop2024.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Collections.Generic;
using HShop2024.Helpers;
using MailKit;
using System.Net.Mail;
using static HShop2024.ViewModels.ReportVM;
using Microsoft.AspNetCore.Authorization;

namespace HShop2024.Controllers
{
    public class QuanLyController : Controller
    {
        private readonly Hshop2023Context _context;

        public QuanLyController(Hshop2023Context context)
        {
            _context = context;
        }

        // Quản lý Khách Hàng
        public IActionResult Index()
        {
            List<KhachHang> khachHangs = _context.KhachHangs.ToList();
            return View(khachHangs);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var khachHang = await _context.KhachHangs.FindAsync(id);
            if (khachHang == null)
            {
                return NotFound();
            }

            return View(khachHang);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(List<KhachHang> khachHangs)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    foreach (var khachHang in khachHangs)
                    {
                        var existingKhachHang = await _context.KhachHangs.FindAsync(khachHang.MaKh);
                        if (existingKhachHang != null)
                        {
                            existingKhachHang.HieuLuc = khachHang.HieuLuc;
                            _context.Update(existingKhachHang);
                        }
                    }
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi cập nhật dữ liệu.");
                }
                return RedirectToAction(nameof(Index));
            }
            return View(khachHangs);
        }

        // Admin Dashboard
        [HttpGet]
        public async Task<IActionResult> DashboardData()
        {
            var dashboardData = new DashboardViewModel
            {
                TotalCustomers = await _context.KhachHangs.CountAsync(),
                TotalEmployees = await _context.NhanViens.CountAsync(),
                TotalRevenue = await _context.HoaDons.CountAsync(),
                Revenue = await _context.HangHoas.CountAsync(),
                ProductRevenue = await _context.NhaCungCaps.CountAsync()
            };

            // Lấy khách hàng có số lượng xu nhiều nhất 
            var topCustomer = await _context.KhachHangs
                .OrderByDescending(k => k.Xu) // Sắp xếp theo số lượng xu
                .Select(k => new DashboardViewModel.CustomerTopSales // Sử dụng lớp CustomerTopSales
                {
                    MaKh = k.MaKh,         // Giả sử có thuộc tính MaKh
                    HoTen = k.HoTen,       // Tên khách hàng
                    SoXu = k.Xu            // Số lượng xu
                })
                .FirstOrDefaultAsync(); // Lấy khách hàng đầu tiên (nhiều nhất)

            // Lưu thông tin khách hàng vào DashboardViewModel
            dashboardData.TopCustomer = topCustomer;


            return View(dashboardData);
        }


        public async Task<IActionResult> Profile()
        {
            var userName = User.Identity.Name;
            var customerIdClaim = HttpContext.User.Claims.SingleOrDefault(p => p.Type == MySetting.CLAIM_CUSTOMERID);

            if (customerIdClaim != null)
            {
                var customerId = customerIdClaim.Value;
            }
            return View();
        }
        // Quản lý Nhân Viên
        public async Task<IActionResult> NhanVienIndex()
        {
            var nhanViens = await _context.NhanViens.ToListAsync();
            return View(nhanViens);
        }

        public async Task<IActionResult> NhanVienDetail(string id, string maNv)
        {
            // Lấy thông tin chi tiết của nhân viên từ id
            var nhanVien = await _context.NhanViens
                .Include(nv => nv.PhanCongs) // Bao gồm cả phân công
                .FirstOrDefaultAsync(nv => nv.MaNv == id);

            if (nhanVien == null)
            {
                return NotFound(); // Trả về trang 404 nếu không tìm thấy nhân viên
            }

            var logins = await _context.LoginHistories
                .Where(l => l.MaNv == id) // Giả sử bạn có bảng LoginHistory lưu lại các lần đăng nhập
                .OrderByDescending(l => l.LoginTime) // Sắp xếp theo thời gian đăng nhập
                .ToListAsync();
            ViewBag.LoginHistory = logins; // Gửi danh sách các lần đăng nhập đến view

            return View(nhanVien);
        }


        public IActionResult NhanVienCreate()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NhanVienCreate([Bind("MaNv,HoTen,Email,MatKhau")] NhanVien nhanVien)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var existingEmployee = await _context.NhanViens
                        .FirstOrDefaultAsync(nv => nv.MaNv == nhanVien.MaNv);

                    if (existingEmployee != null)
                    {
                        ModelState.AddModelError("MaNv", "Mã nhân viên đã tồn tại.");
                    }
                    else
                    {
                        _context.Add(nhanVien);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(NhanVienIndex));
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while creating the employee.");
                }
            }
            return View(nhanVien);
        }
        // GET: NhanVien/Edit/5
        public async Task<IActionResult> NhanVienEdit(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nhanVien = await _context.NhanViens.FindAsync(id);
            if (nhanVien == null)
            {
                return NotFound();
            }
            return View(nhanVien);
        }
        // POST: NhanVien/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NhanVienEdit(string id, [Bind("MaNv,HoTen,Email,MatKhau")] NhanVien nhanVien)
        {
            if (id != nhanVien.MaNv)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(nhanVien);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NhanVienExists(nhanVien.MaNv))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(NhanVienIndex));
            }
            return View(nhanVien);
        }
        // Login cho Nhân Viên
        [HttpGet]
        public IActionResult DangNhap(string? ReturnUrl)
        {
            ViewBag.ReturnUrl = ReturnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DangNhap(LoginNV_VM model, string? ReturnUrl)
        {
            ViewBag.ReturnUrl = ReturnUrl;

            if (ModelState.IsValid)
            {
                if (string.IsNullOrWhiteSpace(model.MaNv))
                {
                    ModelState.AddModelError("MaNv", "Mã nhân viên không được để trống.");
                    return View(model);
                }

                if (string.IsNullOrWhiteSpace(model.MatKhau))
                {
                    ModelState.AddModelError("MatKhau", "Mật khẩu không được để trống.");
                    return View(model);
                }

                var nhanVien = _context.NhanViens.SingleOrDefault(kh =>
                                  kh.MaNv == model.MaNv &&
                                  kh.MatKhau == model.MatKhau); if (nhanVien == null)
                {
                    ModelState.AddModelError("", "Mã nhân viên không đúng hoặc không tồn tại.");
                    return View(model);
                }
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                if (ipAddress == "::1")
                {
                    ipAddress = "127.0.0.1";
                }

                var loginHistory = new LoginHistory
                {
                    MaNv = nhanVien.MaNv,
                    LoginTime = DateTime.Now,
                    IpAddress = ipAddress // Lưu địa chỉ IP
                };


                _context.LoginHistories.Add(loginHistory);
                await _context.SaveChangesAsync();

                if (nhanVien.MatKhau == model.MatKhau)
                {
                    var claims = new List<Claim>
                {
                new Claim(ClaimTypes.Email, nhanVien.Email),
                new Claim(ClaimTypes.Name, nhanVien.HoTen),
                new Claim(MySetting.CLAIM_EMPLOYEEID, nhanVien.MaNv),
                new Claim(ClaimTypes.Role, nhanVien.VaiTro == 2 ? "Admin" : "Employee"),
                new Claim("LastLoginTime", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"))  // Lưu thời gian đăng nhập

                };
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    await HttpContext.SignInAsync(claimsPrincipal);


                    // Cập nhật LastLoginTime
                    nhanVien.LastLoginTime = DateTime.Now;
                    _context.Update(nhanVien);

                    if (Url.IsLocalUrl(ReturnUrl))
                    {
                        return Redirect(ReturnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Profile", "QuanLy");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Mật khẩu không đúng.");
                    return View(model);
                }
            }
            return View();
        }

        private bool NhanVienExists(string id)
        {
            return _context.NhanViens.Any(e => e.MaNv == id);
        }
        public async Task<IActionResult> NhanVienDelete(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nhanVien = await _context.NhanViens
                .FirstOrDefaultAsync(m => m.MaNv == id);
            if (nhanVien == null)
            {
                return NotFound();
            }

            return View(nhanVien);
        }

        // POST: NhanVien/Delete/5
        [HttpPost, ActionName("NhanVienDelete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var nhanVien = await _context.NhanViens.FindAsync(id);
            _context.NhanViens.Remove(nhanVien);
            await _context.SaveChangesAsync();
            return RedirectToAction("NhanVienIndex");
        }
        public async Task<IActionResult> Setting()
        {
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var nhanVien = await _context.NhanViens.SingleOrDefaultAsync(nv => nv.MaNv == userId);

                if (nhanVien != null)
                {

                    ViewBag.EmployeeData = nhanVien;
                }
            }

            return View();
        }
        public async Task<IActionResult> Report()
        {
            var hangHoas = await _context.HangHoas
                .Include(hh => hh.ChiTietHds)
                .ToListAsync();

            var bestSellingProductsReport = hangHoas
                .Select(hh => new ProductSales
                {
                    ProductName = hh.TenHh,
                    SalesCount = hh.ChiTietHds.Sum(ct => ct.SoLuong),
                    TotalRevenue = hh.ChiTietHds.Sum(ct => (ct.SoLuong) * (hh.DonGia ?? 0))
                })
                .OrderByDescending(ps => ps.SalesCount)
                .Take(12)
                .ToList();

            var model = new
            {
                BestSellingProductsReport = new BestSellingProductsViewModel
                {
                    BestSellingProducts = bestSellingProductsReport
                }
            };

            return View(model);
        }
        public IActionResult PhanCong(string maNv)
        {
            var phanCongList = _context.PhanCongs
                .Include(p => p.MaPbNavigation)  // Load thông tin Phòng Ban liên quan
                .Where(p => p.MaNv == maNv && p.HieuLuc == true)  // Lọc theo nhân viên và phân công còn hiệu lực
                .ToList();

            return View(phanCongList);  // Trả về view cùng với danh sách phân công
        }
        [Authorize]
        public async Task<IActionResult> DangXuat()
        {
            await HttpContext.SignOutAsync();
            return Redirect("/");
        }
    }
}