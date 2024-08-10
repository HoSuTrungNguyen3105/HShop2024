using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace HShop2024.ViewModels
{
	public class LoginVM
	{
		[Display(Name = "Tên đăng nhập(Hint:hello)")]
		[Required(ErrorMessage = "Chưa nhập tên đăng nhập")]

		public string UserName { get; set; }

		[Display(Name = "Mật khẩu(Hint:1)")]
		[Required(ErrorMessage = "Chưa nhập mật khẩu")]
		[DataType(DataType.Password)]

		public string Password { get; set; }
	}
}
