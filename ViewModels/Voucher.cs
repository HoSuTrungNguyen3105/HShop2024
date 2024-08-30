namespace HShop2024.ViewModels
{
    public class Voucher
    {
        public string Code { get; set; }
        public decimal DiscountAmount { get; set; } // Giảm giá theo số tiền
        public bool IsPercentage { get; set; } // Nếu là giảm giá theo phần trăm
    }
}
