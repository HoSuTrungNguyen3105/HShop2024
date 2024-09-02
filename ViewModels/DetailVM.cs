using HShop2024.Data;

namespace HShop2024.ViewModels
{
    public class DetailVM
    {
        public int ProductId { get; set; } // Adjust according to your needs
        public string ProductName { get; set; } // Adjust according to your needs
        public decimal ProductPrice { get; set; } // Adjust according to your needs
        public List<HoiDap> HoiDaps { get; set; } = new List<HoiDap>();
    }
}
