namespace HShop2024.ViewModels
{
    public class CartItem
    {
        public int MaHh { get; set; }
        public string Hinh { get; set; }
        public string TenHH { get; set; }
        public double DonGia { get; set; }
        public int SoLuong { get; set; }
        public double ThanhTien => DonGia * SoLuong; // Total price for the item

        public double? GiamGia { get; set; } // Discount amount (if any), nullable if no discount is applied

        public double TongTien
        {
            get
            {
                // Apply discount if available
                var discountAmount = GiamGia ?? 0;
                return ThanhTien - discountAmount;
            }
        }
    }
}
