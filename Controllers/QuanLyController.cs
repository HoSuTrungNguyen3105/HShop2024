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
using static HShop2024.ViewModels.ReportVM;

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

		// Quản lý Nhân Viên
		public async Task<IActionResult> NhanVienIndex()
		{
			var nhanViens = await _context.NhanViens.ToListAsync();
			return View(nhanViens);
		}

		public async Task<IActionResult> NhanVienDetails(string? id)
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

                if (nhanVien.MatKhau == model.MatKhau)
                {
                    var claims = new List<Claim>
                {
                new Claim(ClaimTypes.Email, nhanVien.Email),
                new Claim(ClaimTypes.Name, nhanVien.HoTen),
                new Claim(MySetting.CLAIM_EMPLOYEEID, nhanVien.MaNv),
                new Claim(ClaimTypes.Role, nhanVien.VaiTro == 2 ? "Admin" : "Employee")
                };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

                    if (Url.IsLocalUrl(ReturnUrl))
                    {
                        return Redirect(ReturnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Profile", "KhachHang");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Mật khẩu không đúng.");
                    return View(model);
                }
            }

            return View(model);
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
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Setting()
        {
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var nhanVien = await _context.NhanViens.SingleOrDefaultAsync(nv => nv.MaNv == userId);

                if (nhanVien != null)
                {
                    // Pass employee data to the view if needed
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
    }
}
