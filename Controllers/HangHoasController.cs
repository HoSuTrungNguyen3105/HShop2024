using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HShop2024.Data;
using HShop2024.Helpers;
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
            // Tạo một instance của HangHoaVM và gán các giá trị cho dropdown
            var hangHoaVM = new HangHoaVM
            {
                MaLoaiOptions = _context.Loais
                    .Select(l => new SelectListItem
                    {
                        Value = l.MaLoai.ToString(),
                        Text = l.TenLoai
                    }).ToList(),
                    ManNccOptions = _context.NhaCungCaps
                    .Select(l => new SelectListItem
                    {
                        Value = l.MaNcc.ToString(),
                        Text = l.TenCongTy
                    }).ToList()
            };

			return View(hangHoaVM); // Truyền HangHoaVM vào View
		}

		// POST: HangHoas/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(HangHoaVM hangHoaVM, IFormFile Hinh)
		{
			if (!ModelState.IsValid)
			{
				// Trả lại dữ liệu dropdown khi có lỗi để người dùng không phải chọn lại

				hangHoaVM.MaLoaiOptions = _context.Loais
					.Select(l => new SelectListItem
					{
						Value = l.MaLoai.ToString(),
						Text = l.TenLoai
					}).ToList();
                hangHoaVM.ManNccOptions = _context.NhaCungCaps
                    .Select(l => new SelectListItem
                    {
                        Value = l.MaNcc.ToString(),
                        Text = l.TenCongTy
                    }).ToList();

                return View(hangHoaVM);
			}

			// Chuyển dữ liệu từ HangHoaVM sang HangHoa
			var hangHoa = new HangHoa
			{
				MaHh = hangHoaVM.MaHh,
				TenHh = hangHoaVM.TenHh,
				TenAlias = hangHoaVM.TenAlias,
                MaNcc = hangHoaVM.MaNcc,
				MaLoai = hangHoaVM.MaLoai,
				MoTaDonVi = hangHoaVM.MoTaDonVi,
				DonGia = hangHoaVM.DonGia,
				NgaySx = hangHoaVM.NgaySx,
				GiamGia = hangHoaVM.GiamGia,
				IsOrganic = hangHoaVM.IsOrganic,
				IsFantastic = hangHoaVM.IsFantastic,
				SoLuong = hangHoaVM.SoLuong,
				MoTa = hangHoaVM.MoTa,
				SoLanXem = hangHoaVM.SoLanXem
			};

			// Xử lý file hình ảnh
			if (Hinh != null)
			{
				hangHoa.Hinh = MyUtil.UploadHinh(Hinh, "HangHoa");
			}

			// Lưu xuống database
			try
			{
				_context.Add(hangHoa);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
                ModelState.AddModelError("", "Lỗi khi lưu dữ liệu: " + ex.Message);
                hangHoaVM.MaLoaiOptions = _context.Loais
					.Select(l => new SelectListItem
					{
						Value = l.MaLoai.ToString(),
						Text = l.TenLoai
					}).ToList();
                hangHoaVM.ManNccOptions = _context.NhaCungCaps
                     .Select(l => new SelectListItem
                     {
                         Value = l.MaNcc.ToString(),
                         Text = l.TenCongTy
                     }).ToList();

                return View(hangHoaVM);
			}
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
            ViewBag.OldHinh = hangHoa.Hinh;

            ViewData["MaLoai"] = new SelectList(_context.Loais, "MaLoai", "MaLoai", hangHoa.MaLoai);
            ViewData["MaNcc"] = new SelectList(_context.NhaCungCaps, "MaNcc", "MaNcc", hangHoa.MaNcc);
            return View(hangHoa);
        }

        // POST: HangHoas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaHh,TenHh,TenAlias,MaLoai,MoTaDonVi,DonGia,Hinh,NgaySx,GiamGia,SoLanXem,MoTa,MaNcc,IsOrganic,IsFantastic,SoLuong")] HangHoa hangHoa)
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
            // Kiểm tra nếu Hinh là null, giữ lại hình cũ từ ViewBag
            if (string.IsNullOrEmpty(hangHoa.Hinh))
            {
                hangHoa.Hinh = ViewBag.OldHinh as string;
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

        // GET: HangHoas/Delete/5
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
    .Include(h => h.ChiTietHds)
    .FirstOrDefaultAsync(h => h.MaHh == id);

            if (hangHoa == null)
            {
                return NotFound();
            }

            _context.ChiTietHds.RemoveRange(hangHoa.ChiTietHds);

            _context.HangHoas.Remove(hangHoa);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }

        private bool HangHoaExists(int id)
        {
            return _context.HangHoas.Any(e => e.MaHh == id);
        }
    }
}
