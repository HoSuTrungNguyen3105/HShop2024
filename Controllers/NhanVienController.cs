using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HShop2024.Data; // Đảm bảo rằng bạn sử dụng namespace đúng cho DbContext và model của bạn
using System.Threading.Tasks;
using System.Linq;
using HShop2024.ViewModels;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace HShop2024.Controllers
{
    public class NhanVienController : Controller
    {
        private readonly Hshop2023Context _context;

        public NhanVienController(Hshop2023Context context)
        {
            _context = context;
        }
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DangNhap(LoginVM model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var user = await _context.NhanViens
        //            .FirstOrDefaultAsync(u => u.MaNv == model.MaNv && u.MatKhau == model.MatKhau);

        //        if (user != null)
        //        {
        //            // Xác thực người dùng và gán vai trò nhân viên
        //            var claims = new List<Claim>
        //        {
        //            new Claim(ClaimTypes.Name, user.MaNv),
        //            new Claim(ClaimTypes.Role, "NhanVien")
        //        };

        //            var identity = new ClaimsIdentity(claims, "Login");
        //            var principal = new ClaimsPrincipal(identity);
        //            await HttpContext.SignInAsync(principal);

        //            return RedirectToAction("Index", "Admin");
        //        }

        //        ModelState.AddModelError("", "Thông tin đăng nhập không chính xác.");
        //    }

        //    return View(model);
        //}
        // GET: NhanVien
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

        // GET: NhanVien/Delete/5
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
