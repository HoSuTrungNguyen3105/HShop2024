using static HShop2024.ViewModels.ReportVM;

namespace HShop2024.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalCustomers { get; set; }
        public int TotalEmployees { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal Revenue { get; set; }
        public int ProductRevenue { get; set; }
        public CustomerTopSales TopCustomer { get; set; } // Thêm thuộc tính để lưu thông tin khách hàng
        public class CustomerTopSales
        {
            public string MaKh { get; set; }
            public string HoTen { get; set; }
            public int? SoXu { get; set; } // Số lượng xu
            public override string ToString()
            {
                return $"{HoTen} - Xu: {SoXu}";
            }
        }
        public List<ProductSales> BestSellingProducts { get; set; }
    }

}
