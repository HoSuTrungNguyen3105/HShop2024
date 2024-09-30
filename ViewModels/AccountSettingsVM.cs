using System.ComponentModel.DataAnnotations;

namespace HShop2024.ViewModels
{
    public class AccountSettingsVM
    {
        public string Username { get; set; } // Tên tài khoản
        public string Email { get; set; }    // Email
        public string Phone { get; set; }    // Số điện thoại
        public string Address { get; set; }  // Địa chỉ
        public string NewPassword { get; set; } // Mật khẩu mới (nếu thay đổi)
        public string ConfirmPassword { get; set; } // Xác nhận mật khẩu
        public DateTime Birthdate { get; set; }
        public bool Gender { get; set; }
        public string Xu { get; set; }
    }
}
