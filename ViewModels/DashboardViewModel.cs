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
            public string UserName { get; set; }
            public int? SoXu { get; set; } // Số lượng xu          
        }
        public List<TopSellingProduct> TopSellingProducts { get; set; }
        public class TopSellingProduct
        {
            public int MaHh { get; set; }
            public string TenHh { get; set; }
            public int SoLuotMua { get; set; }
        }
        // Dữ liệu biểu đồ cho khách hàng mới đăng ký
        public List<string> CustomerRegistrationDates { get; set; }
        public List<int> CustomerRegistrations { get; set; } // Số lượng khách hàng đăng ký theo ngày
    }
}
