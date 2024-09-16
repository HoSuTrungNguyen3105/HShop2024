using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HShop2024.Data;

namespace HShop2024.Controllers
{
    public class YeuThichesController : Controller
    {
        private readonly Hshop2023Context _context;

        public YeuThichesController(Hshop2023Context context)
        {
            _context = context;
        }

        // GET: YeuThiches
        public IActionResult Index(string sortOrder)
        {
            Console.WriteLine("sortOrder: " + sortOrder);

            IQueryable<HangHoa> hangHoas = _context.HangHoas;

            // Sắp xếp theo tiêu chí sortOrder
            switch (sortOrder)
            {
                case "isorganic":
                    hangHoas = hangHoas.Where(h => h.IsOrganic).OrderBy(h => h.TenHh);
                    break;
                case "isfantastic":
                    hangHoas = hangHoas.Where(h => h.IsFantastic).OrderBy(h => h.TenHh);
                    break;
                case "mostviewed":
                    hangHoas = hangHoas.OrderByDescending(h => h.SoLanXem);
                    break;
                default:
                    hangHoas = hangHoas.OrderBy(h => h.MaLoai);
                    break;
            }

            var model = hangHoas.ToList();
            return View(model);
        }

        // GET: YeuThiches/Create
        public IActionResult Create(string sortOrder)
        {
            // Lấy tất cả sản phẩm từ cơ sở dữ liệu
            var hangHoas = _context.HangHoas.ToList();

            // Truyền sortOrder đến ViewBag để giữ tùy chọn hiện tại
            ViewBag.SortOrder = sortOrder;

            // Trả về view với dữ liệu
            return View(hangHoas);
        }

        [HttpPost("yeuthiches/addtofavorites")]
        public IActionResult AddToFavorites(int maHh, string sortOrder)
        {
            var hangHoa = _context.HangHoas.Find(maHh);

            if (hangHoa == null)
            {
                return NotFound();
            }

            // Cập nhật các thuộc tính dựa trên sortOrder
            switch (sortOrder)
            {
                case "isorganic":
                    hangHoa.IsOrganic = true;
                    break;
                case "isfantastic":
                    hangHoa.IsFantastic = true;
                    break;
                default:
                    break;
            }

            _context.HangHoas.Update(hangHoa);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }


        // GET: YeuThiches/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var yeuThich = await _context.YeuThiches
                .Include(y => y.MaHhNavigation)
                .Include(y => y.MaKhNavigation)
                .FirstOrDefaultAsync(m => m.MaYt == id);
            if (yeuThich == null)
            {
                return NotFound();
            }

            return View(yeuThich);
        }
         

        // GET: YeuThiches/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var yeuThich = await _context.YeuThiches
                .Include(y => y.MaHhNavigation)
                .Include(y => y.MaKhNavigation)
                .FirstOrDefaultAsync(m => m.MaYt == id);
            if (yeuThich == null)
            {
                return NotFound();
            }

            return View(yeuThich);
        }

        // POST: YeuThiches/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var yeuThich = await _context.YeuThiches.FindAsync(id);
            if (yeuThich != null)
            {
                _context.YeuThiches.Remove(yeuThich);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool YeuThichExists(int id)
        {
            return _context.YeuThiches.Any(e => e.MaYt == id);
        }
    }
}
