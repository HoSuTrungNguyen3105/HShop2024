using HShop2024.Data;
using HShop2024.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using System.Globalization;

namespace HShop2024.Controllers
{
    public class HangHoaController : Controller
    {
        private readonly Hshop2023Context db;

        public HangHoaController(Hshop2023Context conetxt)
        {
            db = conetxt;
        }

		public IActionResult Index(int? loai, string sortOrder)
		{
            IQueryable<HangHoa> hangHoas = db.HangHoas;

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

            // Chọn dữ liệu để trả về cho view
            var result = hangHoas.Select(p => new HangHoaVM
            {
                MaHh = p.MaHh,
                TenHH = p.TenHh,
                DonGia = p.DonGia ?? 0,
                Hinh = p.Hinh ?? "",
                MoTaNgan = p.MoTaDonVi ?? "",
                SoLanXem = p.SoLanXem,
                TenLoai = p.MaLoaiNavigation.TenLoai
            });

			// Trả về view với dữ liệu
			return View(result);
		}
       
        public IActionResult Search(string? query)
        {
            var hangHoas = db.HangHoas.AsQueryable();

            if (query != null)
            {
                hangHoas = hangHoas.Where(p => p.TenHh.Contains(query));
            }

            var result = hangHoas.Select(p => new HangHoaVM
            {
                MaHh = p.MaHh,
                TenHH = p.TenHh,
                DonGia = p.DonGia ?? 0,
                Hinh = p.Hinh ?? "",
                MoTaNgan = p.MoTaDonVi ?? "",
                TenLoai = p.MaLoaiNavigation.TenLoai
            });
            return View(result);
        }

        public IActionResult Detail(int id)
        {
            var product = db.HangHoas
                .Include(p => p.MaLoaiNavigation)
                .SingleOrDefault(p => p.MaHh == id);

            if (product == null)
            {
                TempData["Message"] = $"Không thấy sản phẩm có mã {id}";
                return Redirect("/404");
            }

            var comments = db.HoiDaps
                .Where(h => h.MaHd == id) // Adjust as needed
                .ToList();

            var model = new ChiTietHangHoaVM
            {
                MaHh = product.MaHh,
                TenHH = product.TenHh,
                DonGia = product.DonGia ?? 0,
                ChiTiet = product.MoTa ?? string.Empty,
                Hinh = product.Hinh ?? string.Empty,
                MoTaNgan = product.MoTaDonVi ?? string.Empty,
                TenLoai = product.MaLoaiNavigation.TenLoai,
                SoLuongTon = 10, // Adjust as needed
                DiemDanhGia = 5, // Adjust as needed
                SoLanXem = product.SoLanXem,
                HoiDaps = comments
            };

            return View(model);
        }
    }
}