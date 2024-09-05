using HShop2024.Data;
using HShop2024.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HShop2024.Controllers
{
    public class HangHoaController : Controller
    {
        private readonly Hshop2023Context db;

        public HangHoaController(Hshop2023Context conetxt)
        {
            db = conetxt;
        }

        public IActionResult Index(int? loai)
        {
            var hangHoas = db.HangHoas.AsQueryable();

            if (loai.HasValue)
            {
                hangHoas = hangHoas.Where(p => p.MaLoai == loai.Value);
            }

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