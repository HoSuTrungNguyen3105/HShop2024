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
                .Include(o => o.MaTrangThaiNavigation) // Liên kết với bảng TrangThai
                .OrderByDescending(o => o.NgayDat)
                .ToListAsync();

            // Tạo danh sách ViewModel cho các đơn hàng
            var orderViewModel = orders.Select(order => new OrderVM
            {
                MaHd = order.MaHd,
                UserName = order.UserName,
                NgayDat = order.NgayDat,
                TotalAmount = _context.ChiTietHds
                    .Where(c => c.MaHd == order.MaHd)
                    .Sum(c => (decimal)c.DonGia * c.SoLuong),

                // Lấy mã trạng thái và tên trạng thái từ bảng TrangThai
                MaTrangThai = order.MaTrangThai,
                TenTrangThai = order.MaTrangThaiNavigation.TenTrangThai
            }).ToList();

            // Tính tổng doanh thu
            var totalRevenue = orderViewModel.Sum(o => o.TotalAmount);
            ViewBag.TotalRevenue = totalRevenue;

            // Trả về dữ liệu cho View
            return View(orderViewModel);
        }
        [HttpPost]
        public IActionResult UpdateStatus(string[] selectedOrders, int status)
        {
            if (selectedOrders != null && selectedOrders.Length > 0)
            {
                foreach (var maHd in selectedOrders)
                {
                    // Chuyển đổi maHd từ chuỗi sang int
                    if (int.TryParse(maHd, out int orderId))
                    {
                        var order = _context.HoaDons.Find(orderId); // Tìm kiếm đơn hàng theo mã đơn hàng
                        if (order != null)
                        {
                            order.MaTrangThai = status; // Cập nhật trạng thái
                        }
                    }
                }

                _context.SaveChanges(); // Lưu các thay đổi vào database
            }

            return RedirectToAction("Index"); // Chuyển hướng về trang danh sách đơn hàng
        }
    }
}
