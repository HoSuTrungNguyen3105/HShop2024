using System.ComponentModel.DataAnnotations;

namespace HShop2024.ViewModels
{
    public class AccountSettingsVM
    {
        public string Username { get; set; }
        public string Email { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime Birthdate { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn giới tính")]
        public bool Gender { get; set; }
        public string Xu { get; set; }
    }
}
