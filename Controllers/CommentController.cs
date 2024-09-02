using HShop2024.Data;
using HShop2024.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HShop2024.Controllers
{
    public class CommentController : Controller
    {
        private readonly Hshop2023Context _context; // Update with your actual DbContext

        public CommentController(Hshop2023Context context)
        {
            _context = context;
        }
        
        [HttpPost]
        public IActionResult PostComment(int productId, string review)
        {
            if (User.Identity.IsAuthenticated)
            {
                var currentUser = User.Identity.Name; // Hoặc lấy thông tin khác để xác định nhân viên
                var currentUserId = _context.NhanViens
                                            .Where(nv => nv.HoTen == currentUser) // hoặc điều kiện phù hợp khác
                                            .Select(nv => nv.MaNv)
                                            .FirstOrDefault();
                var newComment = new HoiDap
                {
                    CauHoi = $"Bình luận về sản phẩm {productId}",
                    TraLoi = review,
                    NgayDua = DateOnly.FromDateTime(DateTime.Now),
                    MaNv = currentUserId
                };

                _context.HoiDaps.Add(newComment);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Bình luận đã được gửi thành công!";
                return RedirectToAction("PostComment");
            }
            else
            {
                // Người dùng chưa đăng nhập
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để gửi bình luận.";
                return RedirectToAction("Login", "Account");
            }
        }
    }
}
