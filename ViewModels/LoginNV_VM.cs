using System.ComponentModel.DataAnnotations;

namespace HShop2024.ViewModels
{
    public class LoginNV_VM
    {
        [Display(Name = "Tên đăng nhập")]
        public string MaNv { get; set; } = null!;
        [Display(Name = "Mật khẩu")]
        public string? MatKhau { get; set; }
    }
}
