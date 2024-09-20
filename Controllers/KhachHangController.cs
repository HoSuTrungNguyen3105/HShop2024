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
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
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
                khachhang.HieuLuc = true;
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
                    ModelState.AddModelError("loi", "Tài khoản đã bị khóa . Vui lòng liên hệ Admin để kích hoạt lại.");
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
                            new Claim("Xu", khachHang.Xu.ToString()),
                            new Claim(MySetting.CLAIM_CUSTOMERID, khachHang.MaKh),
                            new Claim(ClaimTypes.Role, "Customer"),
                            new Claim(ClaimTypes.MobilePhone, khachHang.DienThoai ?? ""),
                            new Claim(ClaimTypes.Gender, khachHang.GioiTinh ? "Nam" : "Nữ"),
                            new Claim(ClaimTypes.DateOfBirth, khachHang.NgaySinh.ToString("yyyy-MM-dd")),
                            new Claim("Hinh", khachHang.Hinh ?? ""),
                            new Claim(ClaimTypes.StreetAddress, khachHang.DiaChi ?? "")  // Sử dụng ClaimTypes.StreetAddress
                                            };

                        // Đảm bảo các giá trị không null


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
        [Authorize] // Kiểm tra quyền truy cập nếu cần
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                // Nếu id không hợp lệ, quay lại trang danh sách với thông báo lỗi
                return RedirectToAction("Index", "Admin");
            }

            var khachHang = await db.KhachHangs.FindAsync(id);

            if (khachHang == null)
            {
                // Nếu không tìm thấy tài khoản, quay lại trang danh sách với thông báo lỗi
                TempData["ErrorMessage"] = "Tài khoản không tồn tại.";
                return RedirectToAction("Index", "Admin");
            }

            try
            {
                db.KhachHangs.Remove(khachHang);
                await db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Tài khoản đã được xóa thành công.";
            }
            catch (Exception ex)
            {
                // Xử lý lỗi và thông báo cho người dùng
                TempData["ErrorMessage"] = $"Lỗi xảy ra khi xóa tài khoản: {ex.Message}";
            }

            return RedirectToAction("Index", "Home");
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


        public async Task<IActionResult> Profile()
        {
            var userName = User.Identity.Name;
            var customerIdClaim = HttpContext.User.Claims.SingleOrDefault(p => p.Type == MySetting.CLAIM_CUSTOMERID);

            if (customerIdClaim != null)
            {
                var customerId = customerIdClaim.Value;
                var khachHang = await db.KhachHangs.SingleOrDefaultAsync(kh => kh.MaKh == customerId);

                if (khachHang != null)
                {
                    // Lấy số xu từ session
                    ViewBag.Xu = HttpContext.Session.GetInt32(MySetting.SESSION_XU_KEY) ?? khachHang.Xu ?? 0;
                }
            }

            return View();
        }


        [Authorize]
        public async Task<IActionResult> DangXuat()
        {
            await HttpContext.SignOutAsync();
            return Redirect("/");
        }

  

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var customer = await db.KhachHangs.FirstOrDefaultAsync(c => c.Email == email);
            if (customer == null)
            {
                TempData["ErrorMessage"] = "Email không tồn tại.";
                return RedirectToAction("ForgotPassword");
            }

            // Tạo mã đặt lại mật khẩu
            var resetToken = Guid.NewGuid().ToString();
            customer.RandomKey = resetToken;
            db.Update(customer);
            await db.SaveChangesAsync();

            // Gửi email với mã đặt lại mật khẩu
            var subject = "Đặt lại mật khẩu";
            var resetLink = Url.Action("ResetPassword", "Account", new { token = resetToken }, Request.Scheme);
            var message = $"Bạn đã yêu cầu đặt lại mật khẩu. Vui lòng nhấp vào liên kết sau để đặt lại mật khẩu của bạn: <a href='{resetLink}'>Đặt lại mật khẩu</a>";

            try
            {
                await _emailSender.SendEmailAsync(email, "Đặt lại mật khẩu", "Link đặt lại mật khẩu của bạn");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi gửi email. Vui lòng thử lại.";
                return RedirectToAction("ForgotPassword");
            }

            TempData["SuccessMessage"] = "Đã gửi email để đặt lại mật khẩu.";
            return RedirectToAction("ForgotPassword");
        }


        //[HttpGet]
        //public IActionResult ResetPassword(string token)
        //{
        //	var model = new ResetPasswordViewModel { Token = token };
        //	return View(model);
        //}

        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            var customer = db.KhachHangs.SingleOrDefault(c => c.RandomKey == token);
            if (customer == null)
            {
                TempData["ErrorMessage"] = "Mã đặt lại không hợp lệ.";
                return RedirectToAction("ForgotPassword");
            }

            return View(new ResetPasswordViewModel { Token = token });
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var customer = db.KhachHangs.SingleOrDefault(c => c.RandomKey == model.Token);
                if (customer == null)
                {
                    TempData["ErrorMessage"] = "Mã đặt lại không hợp lệ.";
                    return RedirectToAction("ForgotPassword");
                }

                customer.MatKhau = model.NewPassword.ToMd5Hash(customer.RandomKey);
                customer.RandomKey = null; // Xóa mã đặt lại sau khi đổi mật khẩu
                db.Update(customer);
                await db.SaveChangesAsync();

                TempData["SuccessMessage"] = "Mật khẩu đã được đổi thành công.";
                return RedirectToAction("Login");
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