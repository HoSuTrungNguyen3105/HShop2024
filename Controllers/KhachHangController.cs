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
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using NuGet.Configuration;
using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using static ECommerceMVC.Controllers.KhachHangController;

namespace ECommerceMVC.Controllers
{
    public class KhachHangController : Controller
    {
        private readonly IEmailSender _emailSender;
        private readonly Hshop2023Context db;
        private readonly IMapper _mapper;
        public KhachHangController(Hshop2023Context context, IMapper mapper, IEmailSender emailSender)
        {
            db = context;
            _mapper = mapper;
            _emailSender = emailSender;
        }

        #region Register
        public IActionResult DangKy()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangKy([Bind("MaKh,MatKhau,HoTen,GioiTinh,NgaySinh,DiaChi,DienThoai,Email,Hinh")] KhachHang khachhang, IFormFile Hinh)
        {
            if (ModelState.IsValid)
            {
				// Generate RandomKey
				khachhang.RandomKey = GenerateRandomKey(32); // 32 characters for the random key, you can change this as needed
				if (Hinh != null)
                {
                    khachhang.Hinh = MyUtil.UploadHinh(Hinh, "KhachHang");
                }
                db.Add(khachhang);
                await db.SaveChangesAsync();
                return RedirectToAction("DangNhap", "KhachHang");
            }
            return View(khachhang);
        }

		// Method to generate random key
		private string GenerateRandomKey(int length)
		{
			using (var rng = new RNGCryptoServiceProvider())
			{
				var byteArray = new byte[length];
				rng.GetBytes(byteArray);
				return Convert.ToBase64String(byteArray).Substring(0, length);
			}
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

            // Kiểm tra tính hợp lệ của model
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Kiểm tra xem MaKh và MatKhau có được nhập không
            if (string.IsNullOrWhiteSpace(model.MaKh))
            {
                ModelState.AddModelError("", "Mã khách hàng không được để trống.");
                return View(model);
            }

            if (string.IsNullOrWhiteSpace(model.MatKhau))
            {
                ModelState.AddModelError("", "Mật khẩu không được để trống.");
                return View(model);
            }

            // Tìm khách hàng dựa trên mã khách hàng
            var khachHang = db.KhachHangs.SingleOrDefault(kh =>
                   kh.MaKh == model.MaKh &&
                   kh.MatKhau == model.MatKhau);
            if (khachHang == null)
            {
                ModelState.AddModelError("", "Mã khách hàng hoặc mật khẩu không đúng.");
            }
            else
            {
                if (!khachHang.HieuLuc)
                {
                    ModelState.AddModelError("loi", "Tài khoản mới đăng kí nên chưa được kích hoạt. Vui lòng liên hệ Admin để kích hoạt.");
                }
                else
                {
                    if (khachHang.MatKhau == model.MatKhau.ToMd5Hash(khachHang.RandomKey))
                    {
                        ModelState.AddModelError("ok", "Đăng nhập thành công");
                    }
                    else
                    {
                        var claims = new List<Claim> {
                                new Claim(ClaimTypes.Email, khachHang.Email),
                                new Claim(ClaimTypes.Name, khachHang.HoTen),
                                new Claim(MySetting.CLAIM_CUSTOMERID, khachHang.MaKh),

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
                            return Redirect("Profile");
                        }
                    }
                }

            }
            return View();
        }
        #endregion

        [HttpPost]
        public async Task<IActionResult> Delete(string id) // Thay đổi int thành string
        {
            var khachHang = await db.KhachHangs.FindAsync(id); // Tìm theo khóa chính kiểu string
            if (khachHang != null)
            {
                db.KhachHangs.Remove(khachHang);
                await db.SaveChangesAsync();
            }
            return RedirectToAction("Index", "Admin");
        }

		[HttpPost]
        public async Task<IActionResult> ToggleBan(int id)
        {
            var khachHang = await db.KhachHangs.FindAsync(id);
            if (khachHang != null)
            {
                khachHang.HieuLuc = !khachHang.HieuLuc; // Đảo ngược trạng thái hiệu lực
                await db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }


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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaTaiKhoan()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId != null)
            {
                var khachHang = await db.KhachHangs.FindAsync(userId);

                if (khachHang != null)
                {
                    db.KhachHangs.Remove(khachHang);
                    await db.SaveChangesAsync();

                    // Đăng xuất người dùng
                    await HttpContext.SignOutAsync();
                    return RedirectToAction("Index", "Home");
                }
            }

            return RedirectToAction("Profile");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Tìm người dùng theo email
                var user = db.KhachHangs.FirstOrDefault(kh => kh.Email == model.Email);

                if (user != null)
                {
                    // Tạo token đặt lại mật khẩu (random password)
                    var resetToken = GenerateRandomPassword(8);

                    // Lưu token này vào cơ sở dữ liệu (bạn có thể lưu trữ vào cột 'ResetPasswordToken' và thời gian hết hạn nếu cần)
                    user.RandomKey = resetToken;
                    db.SaveChanges();

                    // Tạo URL đặt lại mật khẩu
                    var resetLink = Url.Action("ResetPassword", "KhachHang", new { token = resetToken }, Request.Scheme);
                    // Gửi email cho người dùng với liên kết đặt lại mật khẩu
                    var subject = "Password Reset Request";
                    var body = $"<p>Please click the following link to reset your password:</p><a href='{resetLink}'>Reset Password</a>";

                    await SendEmailAsync(user.Email, subject, body);

                    return View("ForgotPasswordConfirmation");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Email không tồn tại.");
                }
            }

            return View(model);
        }

        // Action xử lý khi người dùng click vào link đặt lại mật khẩu (GET)
        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Token không hợp lệ.");
            }

            var user = db.KhachHangs.SingleOrDefault(kh => kh.RandomKey == token);
            if (user == null)
            {
                return BadRequest("Token không hợp lệ hoặc đã hết hạn.");
            }

            return View(new ResetPasswordViewModel { Token = token });
        }

        // Action xử lý khi người dùng đặt lại mật khẩu (POST)
        [HttpPost]
        public IActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = db.KhachHangs.SingleOrDefault(kh => kh.RandomKey == model.Token);
                if (user != null)
                {
                    user.MatKhau = HashPassword(model.NewPassword);
                    user.RandomKey = null;
                    db.SaveChanges();

                    return RedirectToAction("ResetPasswordConfirmation");
                }
                else
                {
                    ModelState.AddModelError("", "Token không hợp lệ.");
                }
            }

            return View(model);
        }

        // Hàm để tạo mật khẩu ngẫu nhiên
        private string GenerateRandomPassword(int length)
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder result = new StringBuilder();
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                byte[] uintBuffer = new byte[sizeof(uint)];
                while (length-- > 0)
                {
                    rng.GetBytes(uintBuffer);
                    uint num = BitConverter.ToUInt32(uintBuffer, 0);
                    result.Append(validChars[(int)(num % (uint)validChars.Length)]);
                }
            }
            return result.ToString();
        }

        // Hàm gửi email
        //private async Task SendEmailAsync(string email, string subject, string message)
        //{
        //    var fromAddress = new MailAddress("trungnguyenhs3105@gmail.com", "Nguyen");
        //    var toAddress = new MailAddress(email);
        //    const string fromPassword = "Nguyen31052002";

        //    var smtp = new SmtpClient
        //    {
        //        Host = "smtp.gmail.com",
        //        Port = 587,
        //        EnableSsl = true,
        //        DeliveryMethod = SmtpDeliveryMethod.Network,
        //        UseDefaultCredentials = false,
        //        Credentials = new System.Net.NetworkCredential(fromAddress.Address, fromPassword)
        //    };

        //    using (var mailMessage = new MailMessage(fromAddress, toAddress)
        //    {
        //        Subject = subject,
        //        Body = message,
        //        IsBodyHtml = true
        //    })
        //    {
        //        await smtp.SendMailAsync(mailMessage);
        //    }
        //}
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }
        private string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }
        public async Task<IActionResult> SendEmailAsync(string email, string subject, string message)
        {
            try
            {
                await _emailSender.SendEmailAsync(email, subject, message);
                return Ok("Email sent successfully");
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu cần
                return StatusCode(500, "Error sending email");
            }
        }


    }
}