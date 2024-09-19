using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HShop2024.Data;
using HShop2024.ViewModels;
using HShop2024.Helpers;
using Microsoft.Extensions.Hosting;

namespace HShop2024.Controllers
{
    public class HangHoasController : Controller
    {
        private readonly Hshop2023Context _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HangHoasController(Hshop2023Context context,IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: HangHoas
        public async Task<IActionResult> Index(string searchString)
        {
            var hshop2023Context = _context.HangHoas.Include(h => h.MaLoaiNavigation).Include(h => h.MaNccNavigation);     
            var hangHoas = _context.HangHoas
                                   .Include(h => h.MaLoaiNavigation)
                                   .Include(h => h.MaNccNavigation)
                                   .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                hangHoas = hangHoas.Where(hh => hh.TenHh.Contains(searchString)
                                             || hh.MaHh.ToString().Contains(searchString)
                                             || hh.MaLoaiNavigation.TenLoai.Contains(searchString));
            }

            ViewData["SearchString"] = searchString;
            return View(await hangHoas.ToListAsync());
        }
      
        // GET: HangHoas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hangHoa = await _context.HangHoas
                .Include(h => h.MaLoaiNavigation)
                .Include(h => h.MaNccNavigation)
                .FirstOrDefaultAsync(m => m.MaHh == id);
            if (hangHoa == null)
            {
                return NotFound();
            }

            return View(hangHoa);
        }

        public IActionResult Create()
        {
            ViewData["MaLoai"] = new SelectList(_context.Loais, "MaLoai", "TenLoai");
            ViewData["MaNcc"] = new SelectList(_context.NhaCungCaps, "MaNcc", "TenNcc");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TenHh,TenAlias,MaLoai,MoTaDonVi,DonGia,Hinh,NgaySx,GiamGia,MaNcc,IsOrganic,IsFantastic,SoLuong,SoLanXem")] HangHoaVM hangHoa, IFormFile Hinh)
        {
            if (ModelState.IsValid)
            {
                if (Hinh != null)
                {
                    hangHoa.Hinh = MyUtil.UploadHinh(Hinh, "HangHoa");
                }
                _context.Add(hangHoa);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "HangHoas");
            }
            return View(hangHoa);
        }

        // GET: HangHoas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hangHoa = await _context.HangHoas.FindAsync(id);
            if (hangHoa == null)
            {
                return NotFound();
            }
            ViewData["MaLoai"] = new SelectList(_context.Loais, "MaLoai", "TenLoai", hangHoa.MaLoai);
            ViewData["MaNcc"] = new SelectList(_context.NhaCungCaps, "MaNcc", "TenNcc", hangHoa.MaNcc);
            return View(hangHoa);
        }

        // POST: HangHoas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaHh,TenHh,TenAlias,MaLoai,MoTaDonVi,DonGia,Hinh,NgaySx,GiamGia,SoLanXem,MoTa,MaNcc")] HangHoa hangHoa)
        {
            if (id != hangHoa.MaHh)
            {
                return NotFound();
            }

            if (hangHoa.TenHh.Length > 100)
            {
                ModelState.AddModelError("TenHh", "Tên sản phẩm không được vượt quá 100 ký tự.");
                return View(hangHoa);
            }
            if (!ModelState.IsValid)
            {
                try
                {
                    _context.Update(hangHoa);
                    // Log để kiểm tra dữ liệu trước khi lưu
                    Console.WriteLine($"TenHh: {hangHoa.TenHh}, DonGia: {hangHoa.DonGia}");
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HangHoaExists(hangHoa.MaHh))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                foreach (var value in ModelState.Values)
                {
                    foreach (var error in value.Errors)
                    {
                        Console.WriteLine(error.ErrorMessage);
                    }
                }
                return RedirectToAction(nameof(Index));
            }


            ViewData["MaLoai"] = new SelectList(_context.Loais, "MaLoai", "MaLoai", hangHoa.MaLoai);
            ViewData["MaNcc"] = new SelectList(_context.NhaCungCaps, "MaNcc", "MaNcc", hangHoa.MaNcc);
            return View(hangHoa);
        }

        private bool HangHoaExists(int id)
        {
            return _context.HangHoas.Any(e => e.MaHh == id);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hangHoa = await _context.HangHoas
                .Include(h => h.MaLoaiNavigation)
                .Include(h => h.MaNccNavigation)
                .FirstOrDefaultAsync(m => m.MaHh == id);

            if (hangHoa == null)
            {
                return NotFound();
            }

            return View(hangHoa);
        }

        // POST: HangHoas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hangHoa = await _context.HangHoas
    .Include(h => h.BanBes)
    .Include(h => h.ChiTietHds)
    .FirstOrDefaultAsync(h => h.MaHh == id);

            if (hangHoa == null)
            {
                return NotFound();
            }

            _context.BanBes.RemoveRange(hangHoa.BanBes);
            _context.ChiTietHds.RemoveRange(hangHoa.ChiTietHds);

            _context.HangHoas.Remove(hangHoa);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }
    }
}
