namespace HShop2024.ViewModels
{
	public class CheckoutVM
	{
		public bool GiongKhachHang { get; set; }
		public string? HoTen { get; set; }
		public string? DiaChi { get; set; }
		public string? DienThoai { get; set; }
		public string? GhiChu { get; set; }
		public int SoXu { get; set; } // Thêm thuộc tính SoXu
		public int XuDuocTru { get; set; } // Thêm thuộc tính để giữ số xu đã trừ
	}
}
