namespace HShop2024.ViewModels
{
    public class OrderVM
    {
        public int MaHd { get; set; }
        public string HoTen { get; set; }
        public DateTime NgayDat { get; set; }
        public decimal TotalAmount { get; set; }
        // Thêm các thuộc tính trạng thái
        public int MaTrangThai { get; set; }
        public string TenTrangThai { get; set; }
    }
}
