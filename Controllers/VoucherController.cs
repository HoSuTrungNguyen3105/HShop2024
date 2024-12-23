using Microsoft.AspNetCore.Mvc;
using HShop2024.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using HShop2024.Helpers;

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

		//[HttpPost]
		//public IActionResult SaveVoucher(string voucherCode)
		//{
		//	var customerId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == MySetting.CLAIM_CUSTOMERID)?.Value;

		//	if (customerId == null)
		//	{
		//		return Json(new { success = false, message = "Bạn cần đăng nhập để lưu mã voucher." });
		//	}

		//	// Kiểm tra mã voucher có hợp lệ không
		//	var voucher = _context.Vouchers.SingleOrDefault(v => v.Code == voucherCode);
		//	if (voucher == null ||
		//		(voucher.UsageLimit.HasValue && voucher.TimesUsed.HasValue && voucher.TimesUsed >= voucher.UsageLimit))
		//	{
		//		return Json(new { success = false, message = "Mã voucher không hợp lệ hoặc đã hết hạn sử dụng." });
		//	}

		//	// Kiểm tra mã voucher đã được lưu trước đó chưa
		//	var existingVoucher = _context.SavedVouchers.SingleOrDefault(sv => sv.MaVoucher == voucherCode && sv.CustomerId == customerId);
		//	if (existingVoucher == null)
		//	{
		//		// Tạo mới và lưu mã voucher
		//		var savedVoucher = new SavedVoucher
		//		{
		//			CustomerId = customerId,
		//			MaVoucher = voucherCode,
		//			NgayLuu = DateTime.Now
		//		};
		//		_context.SavedVouchers.Add(savedVoucher);

		//		// Cập nhật số lần sử dụng của voucher
		//		if (voucher.TimesUsed.HasValue)
		//		{
		//			voucher.TimesUsed++;
		//		}
		//		else
		//		{
		//			voucher.TimesUsed = 1;
		//		}

		//		_context.Vouchers.Update(voucher);
		//		_context.SaveChanges();

		//		return Json(new { success = true, message = "Đã lưu mã voucher thành công." });
		//	}

		//	return Json(new { success = false, message = "Mã voucher đã được lưu trước đó." });
		//}


		//public IActionResult GetSavedVouchers()
  //      {
  //          var customerId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == MySetting.CLAIM_CUSTOMERID)?.Value;

  //          if (customerId == null)
  //          {
  //              return Json(new { success = false, message = "Bạn cần đăng nhập để xem mã voucher." });
  //          }

  //          var savedVouchers = _context.SavedVouchers
  //              .Where(sv => sv.CustomerId == customerId)
  //              .Select(sv => new { sv.MaVoucher })
  //              .ToList();

  //          return Json(new { success = true, vouchers = savedVouchers });
  //      }

    }
}
