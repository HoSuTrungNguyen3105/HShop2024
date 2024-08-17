using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HShop2024.Data; 
using System.Threading.Tasks;
using System.Linq;
using HShop2024.ViewModels;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using HShop2024.Helpers;

namespace HShop2024.Controllers
{
    public class NhanVienController : Controller
    {
        private readonly Hshop2023Context _context;

        public NhanVienController(Hshop2023Context context)
        {
            _context = context;
        }
      
        public async Task<IActionResult> Index()
        {
            var nhanViens = await _context.NhanViens.ToListAsync();
            return View(nhanViens);
        }

        // GET: NhanVien/Details/5
        public async Task<IActionResult> Details(string? id)
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

        // GET: NhanVien/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: NhanVien/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaNv,HoTen,Email,MatKhau")] NhanVien nhanVien)
        {
            if (ModelState.IsValid)
            {
                _context.Add(nhanVien);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(nhanVien);
        }

        // GET: NhanVien/Edit/5
        public async Task<IActionResult> Edit(string? id)
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
        public async Task<IActionResult> Edit(string id, [Bind("MaNv,HoTen,Email,MatKhau")] NhanVien nhanVien)
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
                return RedirectToAction(nameof(Index));
            }
            return View(nhanVien);
        }

		#region Login
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
                var nhanVien = _context.NhanViens.SingleOrDefault(nv => nv.MaNv == model.MaNv);
                if (nhanVien != null)
                {
                    // Mã nhân viên không cần random key
                    if (nhanVien.MatKhau == model.MatKhau)
                    {
                        // Đăng nhập thành công cho nhân viên
                        var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, nhanVien.Email),
                    new Claim(ClaimTypes.Name, nhanVien.HoTen),
                    new Claim(MySetting.CLAIM_EMPLOYEEID, nhanVien.MaNv),
                    new Claim(ClaimTypes.Role, "Employee")
                };

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                        await HttpContext.SignInAsync(claimsPrincipal);

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
                        ModelState.AddModelError("loi", "Mật khẩu không đúng.");
                    }
                }

                // Nếu không phải khách hàng cũng không phải nhân viên
                ModelState.AddModelError("loi", "Không có tài khoản này.");
            }

            return View(model);
        }
		#endregion
		public async Task<IActionResult> Delete(string? id)
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
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var nhanVien = await _context.NhanViens.FindAsync(id);
            _context.NhanViens.Remove(nhanVien);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool NhanVienExists(string id)
        {
            return _context.NhanViens.Any(e => e.MaNv == id);
        }

    }
}
