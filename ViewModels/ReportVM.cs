namespace HShop2024.ViewModels
{
    public class ReportVM
    {
        public class ProductSales
        {
            public string ProductName { get; set; }
            public int SalesCount { get; set; }
            public double TotalRevenue { get; set; }
        }

        public class BestSellingProductsViewModel
        {
            public List<ProductSales> BestSellingProducts { get; set; }
        }

    }
}
