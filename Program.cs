using HShop2024.Data;
using HShop2024.Helpers;
using HShop2024.ViewModels;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Owin;
using System.Configuration;


var builder = WebApplication.CreateBuilder(args);
// Cấu hình dịch vụ email SMTP
// Trong Program.cs hoặc Startup.cs
builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>(provider =>
{
	var smtpOptions = new SmtpSettings
	{
		Host = "smtp.gmail.com",
		Port = 587,
		Username = "trungnguyenhs3105@gmail.com",
		Password = "Nguyen31052002",
		EnableSsl = true
	};

	return new SmtpEmailSender(smtpOptions);
});

builder.Services.AddControllersWithViews(); 
builder.Services.AddDbContext<Hshop2023Context>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("HShop"));
});
// Cấu hình SMTP
// Cấu hình từ appsettings.json
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SMTP"));

// Đăng ký dịch vụ SmtpEmailSender
builder.Services.AddSingleton<IEmailSender>(provider =>
{
	var smtpOptions = provider.GetRequiredService<IOptions<SmtpSettings>>().Value;
	return new SmtpEmailSender(smtpOptions);
});
//builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();


// Add session service
builder.Services.AddDistributedMemoryCache(); // Để sử dụng Session trong bộ nhớ
builder.Services.AddSession(options =>
{
	options.IdleTimeout = TimeSpan.FromMinutes(30); // Thời gian sống của session
	options.Cookie.HttpOnly = true; // Bảo mật cookie
	options.Cookie.IsEssential = true; // Cần thiết để session hoạt động đúng
});
// https://docs.automapper.org/en/stable/Dependency-injection.html
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

builder.Logging.AddConsole();

// https://learn.microsoft.com/en-us/aspnet/core/security/authentication/cookie?view=aspnetcore-8.0
builder.Services.AddAuthentication(options =>
{
	options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
	options.LoginPath = "/KhachHang/DangNhap";
	options.LogoutPath = "/KhachHang/Logout";
	options.AccessDeniedPath = "/AccessDenied";
})
.AddGoogle(googleOptions =>
{
	googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"];
	googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
});

// đăng ký PaypalClient dạng Singleton() - chỉ có 1 instance duy nhất trong toàn ứng dụng
builder.Services.AddSingleton(x => new PaypalClient(
        builder.Configuration["PaypalOptions:AppId"],
        builder.Configuration["PaypalOptions:AppSecret"],
        builder.Configuration["PaypalOptions:Mode"]
));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
