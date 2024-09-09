namespace HShop2024.ViewModels
{
    public class CartItem
    {
        public int MaHh { get; set; }
        public string Hinh { get; set; }
        public string TenHH { get; set; }
        public double DonGia { get; set; }
        public int SoLuong { get; set; }
        public double ThanhTien => DonGia * SoLuong; // Tổng tiền của từng sản phẩm
        public double GiamGia { get; set; } // Mã giảm giá (nếu có)
        public double TongTien => ThanhTien - GiamGia; // Tổng tiền sau giảm giá
    }
}
