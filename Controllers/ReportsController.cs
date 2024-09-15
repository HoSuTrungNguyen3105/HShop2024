using HShop2024.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static HShop2024.ViewModels.ReportVM;

namespace HShop2024.Controllers
{
    public class ReportsController : Controller
    {
        private readonly Hshop2023Context _context;

        public ReportsController(Hshop2023Context context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var hangHoas = await _context.HangHoas
                .Include(hh => hh.ChiTietHds)
                .ToListAsync();

            var bestSellingProductsReport = hangHoas
                .Select(hh => new ProductSales
                {
                    ProductName = hh.TenHh,
                    SalesCount = hh.ChiTietHds.Sum(ct => ct.SoLuong ), 
                    TotalRevenue = hh.ChiTietHds.Sum(ct => (ct.SoLuong ) * (hh.DonGia ?? 0))
                })
                .OrderByDescending(ps => ps.SalesCount)
                .Take(9)
                .ToList();

            var model = new
            {
                BestSellingProductsReport = new BestSellingProductsViewModel
                {
                    BestSellingProducts = bestSellingProductsReport
                }
            };

            return View(model);
        }
    }
}
