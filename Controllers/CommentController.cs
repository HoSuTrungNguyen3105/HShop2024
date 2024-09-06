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
        public async Task<IActionResult> PostComment([Bind("CauHoi,TraLoi")] HoiDap hoiDap) { 
            if (ModelState.IsValid)
            {
                hoiDap.NgayDua = DateOnly.FromDateTime(DateTime.Now); // Set the current date
                hoiDap.MaNv = User.Identity.Name; // Assuming the username is used as the MaNv

                _context.Add(hoiDap);
                await _context.SaveChangesAsync();
                return RedirectToAction("Detail", "Product", new { id = hoiDap.MaHd });
            }
            return View(hoiDap);
        }
    }
}
