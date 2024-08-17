using System.ComponentModel.DataAnnotations;

namespace HShop2024.ViewModels
{
    public class LoginNV_VM
    {
        [Display(Name = "Tên đăng nhập(Hint account:hstn)")]
        [Required(ErrorMessage = "Chưa nhập tên đăng nhập")]
        public string MaNv { get; set; } = null!;
        [Display(Name = "Mật khẩu(Password:5)")]
        [Required(ErrorMessage = "Chưa nhập mật khẩu")]
        public string? MatKhau { get; set; }
    }
}
