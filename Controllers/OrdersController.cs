using HShop2024.Data;
using HShop2024.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HShop2024.Controllers
{
    public class OrdersController : Controller
    {
        private readonly Hshop2023Context _context;

        public OrdersController(Hshop2023Context context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Lấy tất cả đơn hàng và sắp xếp theo ngày đặt hàng giảm dần
            var orders = await _context.HoaDons
                .OrderByDescending(o => o.NgayDat) // Sắp xếp theo ngày đặt hàng giảm dần
                .ToListAsync();

            // Tạo danh sách ViewModel cho các đơn hàng
            var orderViewModel = orders.Select(order => new OrderVM
            {
                MaHd = order.MaHd,
                HoTen = order.HoTen,
                NgayDat = order.NgayDat,
                TotalAmount = _context.ChiTietHds
                    .Where(c => c.MaHd == order.MaHd)
                    .Sum(c => (decimal)c.DonGia * c.SoLuong) // Cast to decimal for correct sum
            }).ToList();

            // Tính tổng doanh thu
            var totalRevenue = orderViewModel.Sum(o => o.TotalAmount);
            ViewBag.TotalRevenue = totalRevenue;

            // Trả về dữ liệu cho View
            return View(orderViewModel);
        }

    }
}
