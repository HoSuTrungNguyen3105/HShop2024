using HShop2024.Data;
using HShop2024.ViewModels;
using HShop2024.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

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

            // Retrieve the cart from session
            var gioHang = Cart;

            // Calculate the total price for each item
            foreach (var item in gioHang)
            {
                item.ThanhTien = item.SoLuong * item.DonGia;
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

            ViewBag.PaypalClientdId = _paypalClient.ClientId;
            return View(Cart);
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
                    TempData["Message"] = "Chỉ có khách hàng mới mua hàng được !";
                    return RedirectToAction("Index", "HangHoa");
                }

                var customerId = customerIdClaim.Value;
                var khachHang = db.KhachHangs.SingleOrDefault(kh => kh.MaKh == customerId);

                if (model.GiongKhachHang && khachHang == null)
                {
                    return Json(new { success = false, message = "Khách hàng không tồn tại." });
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

                db.Database.BeginTransaction();
                try
                {
                    db.Add(hoadon);
                    db.SaveChanges();

                    var cthds = new List<ChiTietHd>();
                    if (Cart == null || Cart.Count == 0)
                    {
                        db.Database.RollbackTransaction();
                        return Json(new { success = false, message = "Giỏ hàng rỗng." });
                    }

                    // Lấy danh sách sản phẩm từ giỏ hàng
                    var productIds = Cart.Select(c => c.MaHh).ToList();
                    var products = db.HangHoas.Where(p => productIds.Contains(p.MaHh)).ToList();

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

                        // Tăng số lượng xem của sản phẩm
                        var product = products.SingleOrDefault(p => p.MaHh == item.MaHh);
                        if (product != null)
                        {
                            product.SoLanXem += 1;
                        }
                    }

                    db.AddRange(cthds);
                    db.SaveChanges();
                    db.Database.CommitTransaction();

                    HttpContext.Session.Set<List<CartItem>>(MySetting.CART_KEY, new List<CartItem>());

                    return View("Success");
                }
                catch (Exception ex)
                {
                    db.Database.RollbackTransaction();
                    return StatusCode(500, $"Đã xảy ra lỗi: {ex.Message}");
                }
            }
            return View(Cart);
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

                item.ThanhTien = item.SoLuong * item.DonGia; // Update total price for the item

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
            // Replace with your actual coupon validation logic
            var discount = ValidateCoupon(couponCode);

            if (discount > 0)
            {
                // Store discount information in TempData or Session
                TempData["CouponDiscount"] = discount;
                TempData["CouponMessage"] = "Coupon applied successfully!";
            }
            else
            {
                TempData["CouponMessage"] = "Invalid coupon code.";
            }

            return RedirectToAction("Cart");
        }

        private double ValidateCoupon(string couponCode)
        {
            // Dummy coupon validation
            // Replace this with your actual coupon validation logic
            if (couponCode == "DISCOUNT10")
            {
                return 0.10; // 10% discount
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