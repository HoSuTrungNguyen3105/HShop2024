using System.ComponentModel.DataAnnotations;

namespace HShop2024.ViewModels
{
	public class ResetPasswordViewModel
	{
		[Required]
		public string Token { get; set; }

		[Required(ErrorMessage = "Email là bắt buộc.")]
		[EmailAddress(ErrorMessage = "Định dạng email không hợp lệ.")]
		public string Email { get; set; }

		[Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
		[StringLength(100, ErrorMessage = "Mật khẩu phải có ít nhất {2} ký tự.", MinimumLength = 6)]
		[DataType(DataType.Password)]
		public string Password { get; set; }

		[DataType(DataType.Password)]
		[Display(Name = "Xác nhận mật khẩu")]
		[Compare("Password", ErrorMessage = "Mật khẩu và xác nhận mật khẩu không khớp.")]
		public string ConfirmPassword { get; set; }
	}
}
