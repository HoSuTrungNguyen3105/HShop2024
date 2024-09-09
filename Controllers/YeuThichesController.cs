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
        public async Task<IActionResult> Index()
        {
			var favorites = _context.YeuThiches
			   .Include(y => y.MaHhNavigation)
			   .Include(y => y.MaKhNavigation);
			return View(await favorites.ToListAsync());
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToFavorites(int MaHh, string MaKh, DateTime NgayChon, string MoTa)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var yeuThich = new YeuThich
                    {
                        MaHh = MaHh,
                        MaKh = MaKh,
                        NgayChon = NgayChon,
                        MoTa = MoTa
                    };

                    _context.YeuThiches.Add(yeuThich);
                    await _context.SaveChangesAsync();

                    // Redirect to the product details page
                    return RedirectToAction("Details", "HangHoas", new { id = MaHh });
                }
                catch (Exception ex)
                {
                    // Log the exception (optional)
                    // Handle the exception as needed
                    ModelState.AddModelError("", "An error occurred while adding to favorites.");
                }
            }

            // In case of validation failure, redirect back to the product details page
            return RedirectToAction("Details", "HangHoas", new { id = MaHh });
        }


        // POST: YeuThiches/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaYt,MaHh,MaKh,NgayChon,MoTa")] YeuThich yeuThich)
        {
            if (ModelState.IsValid)
            {
                _context.Add(yeuThich);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaHh"] = new SelectList(_context.HangHoas, "MaHh", "MaHh", yeuThich.MaHh);
            ViewData["MaKh"] = new SelectList(_context.KhachHangs, "MaKh", "MaKh", yeuThich.MaKh);
            return View(yeuThich);
        }

        // GET: YeuThiches/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var yeuThich = await _context.YeuThiches.FindAsync(id);
            if (yeuThich == null)
            {
                return NotFound();
            }
            ViewData["MaHh"] = new SelectList(_context.HangHoas, "MaHh", "MaHh", yeuThich.MaHh);
            ViewData["MaKh"] = new SelectList(_context.KhachHangs, "MaKh", "MaKh", yeuThich.MaKh);
            return View(yeuThich);
        }

        // POST: YeuThiches/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaYt,MaHh,MaKh,NgayChon,MoTa")] YeuThich yeuThich)
        {
            if (id != yeuThich.MaYt)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(yeuThich);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!YeuThichExists(yeuThich.MaYt))
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
            ViewData["MaHh"] = new SelectList(_context.HangHoas, "MaHh", "MaHh", yeuThich.MaHh);
            ViewData["MaKh"] = new SelectList(_context.KhachHangs, "MaKh", "MaKh", yeuThich.MaKh);
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
