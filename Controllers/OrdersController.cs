using HShop2024.Data;
using HShop2024.ViewModels;
using Microsoft.AspNetCore.Mvc;

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
            var orders = _context.HoaDons.ToList();
            var orderViewModel = orders.Select(order => new OrderVM
            {
                MaHd = order.MaHd,
                HoTen = order.HoTen,
                NgayDat = order.NgayDat,
                TotalAmount = _context.ChiTietHds
                    .Where(c => c.MaHd == order.MaHd)
                    .Sum(c => (decimal)c.DonGia * c.SoLuong) // Cast to decimal for correct sum
            }).ToList();

            var totalRevenue = orderViewModel.Sum(o => o.TotalAmount);
            ViewBag.TotalRevenue = totalRevenue;

            return View(orderViewModel);
        }
    }
}
