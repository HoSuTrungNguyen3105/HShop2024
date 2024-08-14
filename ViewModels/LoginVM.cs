using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace HShop2024.ViewModels
{
	public class LoginVM
	{
		[Display(Name = "Tên đăng nhập(Hint account:hello)")]
		[Required(ErrorMessage = "Chưa nhập tên đăng nhập")]

		public string MaKh { get; set; } 

		[Display(Name = "Mật khẩu(Password:1)")]
		[Required(ErrorMessage = "Chưa nhập mật khẩu")]
		[DataType(DataType.Password)]

		public string MatKhau { get; set; }


    }
}
