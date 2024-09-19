using HShop2024.Data;
using HShop2024.ViewModels;
using HShop2024.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System;
using System.Transactions;
using Newtonsoft.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Microsoft.AspNetCore.Authentication;

namespace HShop2024.Controllers
{
    public class CartController : Controller
    {
        private readonly PaypalClient _paypalClient;
        private readonly Hshop2023Context db;
        private readonly ILogger<CartController> _logger;

        public CartController(Hshop2023Context context, PaypalClient paypalClient, ILogger<CartController> logger)
        {
            _paypalClient = paypalClient;
            db = context;
            _logger = logger;

        }

        public List<CartItem> Cart => HttpContext.Session.Get<List<CartItem>>(MySetting.CART_KEY) ?? new List<CartItem>();

        public IActionResult Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                TempData["Message"] = "Quý khách hiện chưa đăng nhập !";
            }
            var userRole = User.Claims.SingleOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            if (userRole != null && userRole == "Employee")
            {
                TempData["Message"] = "Nhân viên và Admin không thể thực hiện giao dịch!";
            }

            var gioHang = Cart;

            if (TempData["CouponDiscount"] != null && double.TryParse(TempData["CouponDiscount"].ToString(), out double discount))
            {
                ViewBag.CouponDiscount = discount;
            }
            else
            {
                ViewBag.CouponDiscount = 0;
            }
            return View(gioHang);
        }


        [HttpPost]
        public JsonResult AddToCart(int id, int quantity = 1)
        {
            var gioHang = Cart;
            var item = gioHang.SingleOrDefault(p => p.MaHh == id);
            if (item == null)
            {
                var hangHoa = db.HangHoas.SingleOrDefault(p => p.MaHh == id);
                if (hangHoa == null)
                {
                    return Json(new { success = false, message = $"Không tìm thấy hàng hóa có mã {id}" });
                }
                item = new CartItem
                {
                    MaHh = hangHoa.MaHh,
                    TenHH = hangHoa.TenHh,
                    DonGia = hangHoa.DonGia ?? 0,
                    Hinh = hangHoa.Hinh ?? string.Empty,
                    SoLuong = quantity
                };
                gioHang.Add(item);
            }
            else
            {
                item.SoLuong += quantity;
            }

            HttpContext.Session.Set(MySetting.CART_KEY, gioHang);
            return Json(new { success = true, cartCount = gioHang.Sum(p => p.SoLuong) });
        }

        public IActionResult RemoveCart(int id)
        {
            var gioHang = Cart;
            var item = gioHang.SingleOrDefault(p => p.MaHh == id);
            if (item != null)
            {
                gioHang.Remove(item);
                HttpContext.Session.Set(MySetting.CART_KEY, gioHang);
            }
            return RedirectToAction("Index");
        }

        [Authorize]
        [HttpGet]
        public IActionResult Checkout()
        {
            if (Cart.Count == 0)
            {
                TempData["Message"] = "Mời bạn chọn sản phẩm!";
                return RedirectToAction("Index", "HangHoa");
            }

            // Lấy mã giảm giá từ Session
            var appliedCoupon = HttpContext.Session.GetString("AppliedCoupon");
            if (!string.IsNullOrEmpty(appliedCoupon))
            {
                ViewBag.AppliedCoupon = appliedCoupon;
                var discount = ValidateCoupon(appliedCoupon);
                ViewBag.CouponDiscount = discount;
            }
            else
            {
                ViewBag.CouponDiscount = 0;
            }

            ViewBag.PaypalClientId = _paypalClient.ClientId;
            return View(Cart);
        }
        // Action method to get invoice details
        [HttpGet]
        public async Task<IActionResult> GetInvoiceDetails(int maHd)
        {
            // Fetch the invoice details based on MaHd
            var invoice = await db.HoaDons
                .Where(i => i.MaHd == maHd)
                .Select(i => new
                {
                    maHd = i.MaHd,
                    maKh = i.MaKh,
                    cachtt = i.NgayDat,
                    diachi = i.DiaChi,
                    phivc = i.NgayCan,
                    ngaygiao = i.NgayGiao,
                    dienthoai = i.DienThoai
                })
                .FirstOrDefaultAsync();

            if (invoice == null)
            {
                return NotFound();
            }

            return Json(invoice);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Checkout(CheckoutVM model)
        {
            if (ModelState.IsValid)
            {
                var customerIdClaim = HttpContext.User.Claims.SingleOrDefault(p => p.Type == MySetting.CLAIM_CUSTOMERID);

                if (customerIdClaim == null)
                {
                    TempData["Message"] = "Chỉ có khách hàng mới mua hàng được!";
                    return RedirectToAction("Index", "HangHoa");
                }
                
                var customerId = customerIdClaim.Value;
                var khachHang = db.KhachHangs.SingleOrDefault(kh => kh.MaKh == customerId);

                if (model.GiongKhachHang && khachHang == null)
                {
                     TempData["Message"] = "Khách hàng không tồn tại." ;
                    return RedirectToAction("Index", "HangHoa");
                }

                var hoadon = new HoaDon
                {
                    MaKh = customerId,
                    HoTen = model.HoTen ?? khachHang?.HoTen,
                    DiaChi = model.DiaChi ?? khachHang?.DiaChi,
                    DienThoai = model.DienThoai ?? khachHang?.DienThoai,
                    NgayDat = DateTime.Now,
                    CachThanhToan = "COD",
                    CachVanChuyen = "GRAB",
                    MaTrangThai = 0,
                    GhiChu = model.GhiChu
                };

                using (var transaction = await db.Database.BeginTransactionAsync())
                {
                    try
                    {
                        db.Add(hoadon);
                        await db.SaveChangesAsync();

                        if (Cart == null || Cart.Count == 0)
                        {
                            await transaction.RollbackAsync();
                            return Json(new { success = false, message = "Giỏ hàng rỗng." });
                        }

                        var productIds = Cart.Select(c => c.MaHh).ToList();
                        var products = db.HangHoas.Where(p => productIds.Contains(p.MaHh)).ToList();

                        var cthds = new List<ChiTietHd>();

                        foreach (var item in Cart)
                        {
                            cthds.Add(new ChiTietHd
                            {
                                MaHd = hoadon.MaHd,
                                SoLuong = item.SoLuong,
                                DonGia = item.DonGia,
                                MaHh = item.MaHh,
                                GiamGia = 0
                            });
                            var product = products.SingleOrDefault(p => p.MaHh == item.MaHh);
                            if (product != null)
                            {
                                product.SoLanXem += 1;
                                // Giảm số lượng hàng hóa
                                if (product.SoLuong >= item.SoLuong)
                                {
                                    product.SoLuong -= item.SoLuong;
                                }
                                else
                                {
                                    // Nếu số lượng không đủ, có thể thêm thông báo lỗi hoặc xử lý theo cách khác
                                    TempData["ErrorMessage"] = $"Không đủ số lượng hàng hóa với mã {item.MaHh}.";
                                    await transaction.RollbackAsync();
                                    return View("Error");
                                }
                            }                          
                        }

                        db.AddRange(cthds);
                        await db.SaveChangesAsync();

                        if (khachHang != null)
                        {
                            khachHang.Xu = (khachHang.Xu ?? 0) + 100; // Cộng thêm 100 xu
                            db.Update(khachHang);
                            await db.SaveChangesAsync(); // Lưu thay đổi vào cơ sở dữ liệu

                            // Cập nhật lại claim để chứa thông tin xu mới
                            var identity = (ClaimsIdentity)User.Identity;
                            var oldXuClaim = identity.FindFirst("Xu");
                            if (oldXuClaim != null)
                            {
                                identity.RemoveClaim(oldXuClaim);
                            }
                            identity.AddClaim(new Claim("Xu", khachHang.Xu.ToString()));

                            await HttpContext.SignInAsync(new ClaimsPrincipal(identity)); // Cập nhật claims

                            HttpContext.Session.SetInt32(MySetting.SESSION_XU_KEY, khachHang.Xu ?? 0);
                        }

                        await transaction.CommitAsync(); // Commit transaction nếu mọi thứ OK
                        HttpContext.Session.Set<List<CartItem>>(MySetting.CART_KEY, new List<CartItem>());
                        TempData["XuMessage"] = "Bạn đã nhận được 100 xu cộng thêm vào tài khoản của bạn.";
                        return View("Success");
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync(); // Rollback transaction nếu có lỗi
                                                           // Log lỗi và xử lý ngoại lệ
                        TempData["ErrorMessage"] = "Có lỗi xảy ra khi xử lý đơn hàng.";
                        return View("Error");
                    }
                }
            }
            return View(Cart);
        }
        [HttpPost]
        public IActionResult ApplyVoucher(string voucherCode)
        {
            //var voucher = db.Vouchers.SingleOrDefault(v => v.Code == voucherCode && v.HieuLuc);
            var voucher = db.Vouchers.SingleOrDefault(v => v.Code == voucherCode );

            if (voucher != null)
            {
                HttpContext.Session.SetString("VoucherCode", voucher.Code);
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Mã voucher không hợp lệ hoặc đã hết hạn." });
        }

        private List<CartItem> GetCartItems()
        {
            // Example logic to get cart items from session or database
            var cartItemsJson = HttpContext.Session.GetString("CartItems");
            if (string.IsNullOrEmpty(cartItemsJson))
            {
                return new List<CartItem>(); // Return empty list if no cart items found
            }

            // Deserialize JSON to List<CartItem>
            var cartItems = JsonConvert.DeserializeObject<List<CartItem>>(cartItemsJson);
            return cartItems;
        }

        [HttpPost]
        public IActionResult UpdateCartQuantity(int id, int quantity, string action)
        {
            var gioHang = Cart;
            var item = gioHang.SingleOrDefault(p => p.MaHh == id);
            if (item != null)
            {
                if (action == "increase")
                {
                    item.SoLuong += 1; // Increase quantity by 1
                }
                else if (action == "decrease" && item.SoLuong > 1)
                {
                    item.SoLuong -= 1; // Decrease quantity by 1
                }


                // Update cart in session
                HttpContext.Session.Set(MySetting.CART_KEY, gioHang);
            }

            // Calculate subtotal and total
            var subtotal = gioHang.Sum(p => p.ThanhTien);
            var total = subtotal; // Assuming no shipping cost
            var totalAmount = total;

            // Return updated quantities, prices, and totals
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ApplyCoupon(string couponCode)
        {
            var discount = ValidateCoupon(couponCode);

            if (discount > 0)
            {
                TempData["CouponDiscount"] = discount.ToString("0.00");
                TempData["CouponMessage"] = "Mã giảm giá đã được áp dụng thành công!";
            }
            else
            {
                TempData["CouponMessage"] = "Mã giảm giá không hợp lệ.";
            }

            return RedirectToAction("Index", "Cart");
        }


        private double ValidateCoupon(string couponCode)
        {
            // Chuyển đổi mã giảm giá thành chữ hoa
            var upperCouponCode = couponCode.ToUpper();

            // Truy vấn cơ sở dữ liệu
            var voucher = db.Vouchers
                .FirstOrDefault(v => v.Code.ToUpper() == upperCouponCode);

            if (voucher != null && voucher.DiscountAmount > 0)
            {
                double discountAmount = (double)voucher.DiscountAmount;
                return voucher.IsPercentage ? discountAmount / 100.0 : discountAmount;
            }

            return 0;
        }


        [Authorize]
        public IActionResult PaymentSuccess()
        {
            return View("Success");
        }

        #region Paypal payment
        [Authorize]
        [HttpPost("/Cart/create-paypal-order")]
        public async Task<IActionResult> CreatePaypalOrder(CancellationToken cancellationToken)
        {
            // Thông tin đơn hàng gửi qua Paypal
            var tongTien = Cart.Sum(p => p.ThanhTien).ToString();
            var donViTienTe = "USD";
            var maDonHangThamChieu = "DH" + DateTime.Now.Ticks.ToString();

            try
            {
                var response = await _paypalClient.CreateOrder(tongTien, donViTienTe, maDonHangThamChieu);

                return Ok(response);
            }
            catch (Exception ex)
            {
                var error = new { ex.GetBaseException().Message };
                return BadRequest(error);
            }
        }

        [Authorize]
        [HttpPost("/Cart/capture-paypal-order")]
        public async Task<IActionResult> CapturePaypalOrder(string orderID, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _paypalClient.CaptureOrder(orderID);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var error = new { ex.GetBaseException().Message };
                return BadRequest(error);
            }
        }
        #endregion
    }
}