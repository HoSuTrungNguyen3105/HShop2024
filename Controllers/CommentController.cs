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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostComment(int name, string review)
        {
            if (string.IsNullOrWhiteSpace(review))
            {
                TempData["ErrorMessage"] = "Bình luận không được để trống.";
                return RedirectToAction("Detail", "HangHoa", new { id = name });
            }

            var comment = new HoiDap
            {
                MaHh = name,
                CauHoi = review,
                NgayDua = DateOnly.FromDateTime(DateTime.Now),
            };


            _context.HoiDaps.Add(comment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Bình luận của bạn đã được đăng.";
            return RedirectToAction("Detail", "HangHoa", new { id = name });
        }

    }
}
