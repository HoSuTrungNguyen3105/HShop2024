namespace HShop2024.ViewModels
{
	public class CheckoutVM
	{
		public bool GiongKhachHang { get; set; }
		public string? UserName { get; set; }
		public string? DiaChi { get; set; }
		public string? PhoneNumber { get; set; }
		public string? GhiChu { get; set; }
		public int SoXu { get; set; } // Thêm thuộc tính SoXu
		public int XuDuocTru { get; set; } // Thêm thuộc tính để giữ số xu đã trừ
	}
}
