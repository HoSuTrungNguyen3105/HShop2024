
using AutoMapper;
using HShop2024.Data;
using HShop2024.Helpers;
using HShop2024.Services;
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
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using NuGet.Configuration;
using NuGet.Protocol;
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
        private readonly ILogger<KhachHangController> _logger;
        private UserManager<KhachHang> _userManager;
        private SignInManager<KhachHang> _signInManager;
        public KhachHangController(Hshop2023Context context, IMapper mapper, IEmailSender emailSender, UserManager<KhachHang> userManager, ILogger<KhachHangController> logger,
        SignInManager<KhachHang> signInManager)
        {
            db = context;
            _mapper = mapper;
            _emailSender = emailSender;
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _signInManager = signInManager;
            _logger = logger;
        }
        #region Register
        public IActionResult DangKy()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangKy(KhachHang khachhang)
        {
            if (ModelState.IsValid)
            {
                
                db.Add(khachhang);
                await db.SaveChangesAsync();
                return RedirectToAction("DangNhap", "KhachHang");
            }
            return View(khachhang);
        }


        // Phương thức tạo RandomKey
        private string GenerateRandomKey(int length)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var byteArray = new byte[length];
                rng.GetBytes(byteArray);

                // Chuyển byteArray thành chuỗi Base64 và giới hạn độ dài
                return Convert.ToBase64String(byteArray).Substring(0, length);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> UpdateProfile(AccountSettingsVM model)
        {
            if (!ModelState.IsValid)
            {
                return View("Profile", model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await db.KhachHangs.FindAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            // Cập nhật thông tin người dùng
            user.UserName = model.Username;
            user.Email = model.Email;
            user.PhoneNumber = model.Phone;
            user.DiaChi = model.Address;
            user.NgaySinh = model.Birthdate;
            user.GioiTinh = model.Gender;

            // Lưu thay đổi vào database
            await db.SaveChangesAsync();

            // Cập nhật claims
            var identity = (ClaimsIdentity)User.Identity;
            UpdateClaim(identity, ClaimTypes.Name, model.Username);
            UpdateClaim(identity, ClaimTypes.Email, model.Email);
            UpdateClaim(identity, ClaimTypes.MobilePhone, model.Phone);
            UpdateClaim(identity, ClaimTypes.StreetAddress, model.Address);
            UpdateClaim(identity, ClaimTypes.DateOfBirth, model.Birthdate.ToString("yyyy-MM-dd"));
            UpdateClaim(identity, ClaimTypes.Gender, model.Gender ? "Nam" : "Nữ");
            foreach (var claim in identity.Claims)
            {
                Console.WriteLine($"Claim Type: {claim.Type}, Value: {claim.Value}");
            }

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
            return RedirectToAction("Profile");
        }

        private void UpdateClaim(ClaimsIdentity identity, string claimType, string newValue)
        {
            // Tìm claim hiện tại và xóa nó
            var existingClaim = identity.FindFirst(claimType);
            if (existingClaim != null)
            {
                identity.RemoveClaim(existingClaim);
            }

            // Thêm claim mới với giá trị cập nhật
            identity.AddClaim(new Claim(claimType, newValue));

            // Cập nhật lại ClaimsPrincipal (tùy thuộc vào việc bạn dùng cái gì để quản lý user hiện tại)
            var userPrincipal = new ClaimsPrincipal(identity);
            HttpContext.User = userPrincipal;
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
            var khachhang = await db.KhachHangs.FindAsync();
            if (ModelState.IsValid)

            {
                model.MatKhau = khachhang.PasswordHash;
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.MatKhau, false, false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Home", "Index"); // hoặc trang nào đó sau khi đăng nhập thành công
                }

                ModelState.AddModelError("", "Sai tài khoản hoặc mật khẩu");
            }

            return View();
        }
        #endregion

        //[HttpPost]
        //public async Task<IActionResult> ToggleBan(int id)
        //{
        //    var khachHang = await db.KhachHangs.FindAsync(id);
        //    if (khachHang != null)
        //    {
        //        khachHang.HieuLuc = !khachHang.HieuLuc; // Đảo ngược trạng thái hiệu lực
        //        await db.SaveChangesAsync();
        //    }
        //    return RedirectToAction("Index");
        //}

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

                customer.PasswordHash = model.Password.ToMd5Hash(customer.RandomKey);
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
