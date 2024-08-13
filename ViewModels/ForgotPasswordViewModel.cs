using System.ComponentModel.DataAnnotations;

namespace HShop2024.ViewModels
{
	public class ForgotPasswordViewModel
	{
		[Required(ErrorMessage = "Email là bắt buộc.")]
		[EmailAddress(ErrorMessage = "Định dạng email không hợp lệ.")]
		public string Email { get; set; }
	}
}
