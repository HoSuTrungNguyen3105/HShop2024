using System.ComponentModel.DataAnnotations;

namespace HShop2024.ViewModels
{
    public class LoginNV_VM
    {
        [Display(Name = "Tên đăng nhập(Hint account:hstn)")]
        public string MaNv { get; set; } = null!;
        [Display(Name = "Mật khẩu(Password:5)")]
        public string? MatKhau { get; set; }
    }
}
