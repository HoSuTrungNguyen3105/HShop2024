using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HShop2024.Data;
using HShop2024.ViewModels;

namespace HShop2024.Controllers
{
    public class HangHoasController : Controller
    {
        private readonly Hshop2023Context _context;

        public HangHoasController(Hshop2023Context context)
        {
            _context = context;
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

        // GET: HangHoas/Create
        public IActionResult Create()
        {
            ViewData["MaLoai"] = new SelectList(_context.Loais, "MaLoai", "TenLoai"); // Hiển thị tên loại thay vì mã
            ViewData["MaNcc"] = new SelectList(_context.NhaCungCaps, "MaNcc", "TenNcc"); // Hiển thị tên NCC thay vì mã
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaHh,TenHh,TenAlias,MaLoai,MoTaDonVi,DonGia,NgaySx,GiamGia,SoLanXem,MoTa,MaNcc")] HangHoa hangHoa, IFormFile Hinh)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Xử lý lưu hình ảnh
                    if (Hinh != null && Hinh.Length > 0)
                    {
                        var fileName = Path.GetFileName(Hinh.FileName);
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Hinh/HangHoa", fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await Hinh.CopyToAsync(stream);
                        }

                        // Lưu đường dẫn hình ảnh vào thuộc tính Hinh
                        hangHoa.Hinh = "/Hinh/HangHoa" + fileName;
                    }

                    // Thêm hàng hóa vào cơ sở dữ liệu
                    _context.Add(hangHoa);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Lỗi xảy ra khi tạo mới hàng hóa: {ex.Message}");
            }          

            ViewData["MaLoai"] = new SelectList(_context.Loais, "MaLoai", "TenLoai", hangHoa.MaLoai);
            ViewData["MaNcc"] = new SelectList(_context.NhaCungCaps, "MaNcc", "TenNcc", hangHoa.MaNcc);
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
