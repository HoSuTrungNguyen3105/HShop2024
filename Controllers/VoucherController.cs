using Microsoft.AspNetCore.Mvc;
using HShop2024.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace HShop2024.Controllers
{
    public class VoucherController : Controller
    {
        private readonly Hshop2023Context _context;

        public VoucherController(Hshop2023Context context)
        {
            _context = context;
        }

        // Action để trả về danh sách Voucher
        public async Task<IActionResult> Index()
        {
            var vouchers = await _context.Vouchers.ToListAsync();
            return View(vouchers);
        }

        // Phần hiển thị Voucher dưới dạng Partial View
        public async Task<IActionResult> List()
        {
            var vouchers = await _context.Vouchers.ToListAsync();
            return PartialView("_VoucherPartial", vouchers);
        }

    }
}
