using AutoMapper;
using HShop2024.Data;
using HShop2024.Helpers;
using HShop2024.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Configuration;
using System.Globalization;
using System.Security.Claims;
using static ECommerceMVC.Controllers.KhachHangController;

namespace ECommerceMVC.Controllers
{
	public class KhachHangController : Controller
	{
		private readonly Hshop2023Context db;
		private readonly IMapper _mapper;
		private readonly UserManager<KhachHang> _userManager;
		private readonly IEmailSender _emailSender;
		public KhachHangController(Hshop2023Context context, IMapper mapper)
		{
			db = context;
			_mapper = mapper;
		}

		#region Register
		[HttpGet]
		public IActionResult DangKy()
		{
			return View();
		}
		//[HttpPost]
		//public IActionResult DangKy (RegisterVM model, IFormFile Hinh)
		//{
		//	if (ModelState.IsValid)
		//	{
		//		try
		//		{
		//			var khachHang = _mapper.Map<KhachHang>(model);
		//			khachHang.RandomKey = MyUtil.GenerateRamdomKey();
		//			khachHang.MatKhau = model.MatKhau.ToMd5Hash(khachHang.RandomKey);
		//			khachHang.HieuLuc = true; // Sẽ xử lý khi dùng Mail để active
		//			khachHang.VaiTro = 0;

		//			if (Hinh != null)
		//			{
		//				khachHang.Hinh = MyUtil.UploadHinh(Hinh, "KhachHang");
		//			}

		//			db.Add(khachHang);
		//			db.SaveChangesAsync();

		//			// Redirect to a success page or display a success message
		//			return RedirectToAction("Index", "HangHoa");
		//		}
		//		catch (Exception ex)
		//		{
		//			// Handle exception
		//			ModelState.AddModelError("", "Đã có lỗi xảy ra. Vui lòng thử lại sau.");
		//			return View(model);
		//		}
		//	}

		//	// Return view with validation errors
		//	return View(model);
		//}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DangKy([Bind("MaKh,MatKhau,HoTen,GioiTinh,NgaySinh,DiaChi,DienThoai,Email,Hinh")] KhachHang khachHang,RegisterVM model)
		{
			if (ModelState.IsValid)
			{
				db.Add(khachHang);
				await db.SaveChangesAsync();
				ModelState.AddModelError("loi", "Đã có khách hàng được thêm vào");
			}
			return View(model);
		}

		#endregion

		#region Login
		[HttpGet]
		public IActionResult DangNhap(string? ReturnUrl)
		{
			ViewBag.ReturnUrl = ReturnUrl;
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> DangNhap(LoginVM model, string? ReturnUrl)
		{
			ViewBag.ReturnUrl = ReturnUrl;
			if (ModelState.IsValid)
			{
				var khachHang = db.KhachHangs.SingleOrDefault(kh => kh.MaKh == model.MatKhau);
				if (khachHang == null)
				{
					ModelState.AddModelError("loi", "Không có khách hàng này");
				}
				else
				{
					if (!khachHang.HieuLuc)
					{
						ModelState.AddModelError("loi", "Tài khoản đã bị khóa. Vui lòng liên hệ Admin.");
					}
					else
					{
						if (khachHang.MatKhau != model.MatKhau.ToMd5Hash(khachHang.RandomKey))
						{
							ModelState.AddModelError("loi", "Sai thông tin đăng nhập");
						}
						else
						{
							var claims = new List<Claim> {
								new Claim(ClaimTypes.Email, khachHang.Email),
								new Claim(ClaimTypes.Name, khachHang.HoTen),
								new Claim("CustomerID", khachHang.MaKh),

								//claim - role động
								new Claim(ClaimTypes.Role, "Customer")
							};

							var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
							var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

							await HttpContext.SignInAsync(claimsPrincipal);

							if (Url.IsLocalUrl(ReturnUrl))
							{
								return Redirect(ReturnUrl);
							}
							else
							{
								return Redirect("/");
							}
						}
					}
				}
			}
			return View();
		}
		#endregion

		[Authorize]
		public IActionResult Profile()
		{
			return View();
		}

		[Authorize]
		public async Task<IActionResult> DangXuat()
		{
			await HttpContext.SignOutAsync();
			return Redirect("/");
		}
		// Hiển thị form quên mật khẩu
		[HttpGet]
		public IActionResult ForgotPassword()
		{
			return View();
		}

		// Xử lý form quên mật khẩu
		[HttpPost]
		public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
		{
			if (ModelState.IsValid)
			{
				var user = await _userManager.FindByEmailAsync(model.Email);
				if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
				{
					return RedirectToAction("ForgotPasswordConfirmation");
				}

				var token = await _userManager.GeneratePasswordResetTokenAsync(user);
				var callbackUrl = Url.Action("ResetPassword", "Account", new { token, email = user.Email }, Request.Scheme);

				await _emailSender.SendEmailAsync(
					model.Email,
					"Đặt lại mật khẩu",
					$"Vui lòng đặt lại mật khẩu của bạn bằng cách <a href='{callbackUrl}'>click vào đây</a>.");

				return RedirectToAction("ForgotPasswordConfirmation");
			}

			return View(model);
		}

		// Xác nhận đã gửi email quên mật khẩu
		[HttpGet]
		public IActionResult ForgotPasswordConfirmation()
		{
			return View();
		}

		// Hiển thị form đặt lại mật khẩu
		[HttpGet]
		public IActionResult ResetPassword(string token = null)
		{
			if (token == null)
			{
				return BadRequest("Mã token để đặt lại mật khẩu phải được cung cấp.");
			}
			return View(new ResetPasswordViewModel { Token = token });
		}

		// Xử lý form đặt lại mật khẩu
		[HttpPost]
		public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
		{
			if (ModelState.IsValid)
			{
				var user = await _userManager.FindByEmailAsync(model.Email);
				if (user == null)
				{
					return RedirectToAction("ResetPasswordConfirmation");
				}

				var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
				if (result.Succeeded)
				{
					return RedirectToAction("ResetPasswordConfirmation");
				}

				foreach (var error in result.Errors)
				{
					ModelState.AddModelError(string.Empty, error.Description);
				}
			}

			return View(model);
		}

		// Xác nhận đã đặt lại mật khẩu thành công
		[HttpGet]
		public IActionResult ResetPasswordConfirmation()
		{
			return View();
		}
	}
}