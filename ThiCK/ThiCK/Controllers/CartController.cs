using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThiCK.Models;
using ThiCK.Models.ViewModels;
using ThiCK.Repository;
using Newtonsoft.Json;
using System.Diagnostics;

namespace ThiCK.Controllers
{
	public class CartController : Controller
	{
		private readonly DataContext _dataContext;
		public CartController(DataContext _context)
		{
			_dataContext = _context;
		}
		public IActionResult Index()
		{
			List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();

			// Nhận giá shipping từ cookie
			var shippingPriceCookie = Request.Cookies["ShippingPrice"];
			decimal shippingPrice = 0;
			if (shippingPriceCookie != null)
			{
				var shippingPriceJson = shippingPriceCookie;
				shippingPrice = JsonConvert.DeserializeObject<decimal>(shippingPriceJson);
			}
			CartItemViewModel cartVM = new()
			{
				CartItems = cartItems,
				GrandTotal = cartItems.Sum(x => x.Quantity * x.Price),
				ShippingCost = shippingPrice
			};
			return View(cartVM);
		}

		public IActionResult Checkout()
		{
			// Xóa phí ship bằng cách đặt cookie "ShippingPrice" thành 0
			try
			{
				var shippingPriceJson = JsonConvert.SerializeObject(0m); // 0m là số thập phân (decimal)
				var cookieOptions = new CookieOptions
				{
					HttpOnly = true,
					Expires = DateTimeOffset.UtcNow.AddMinutes(30),
					Secure = true // Using HTTPS
				};
				Response.Cookies.Append("ShippingPrice", shippingPriceJson, cookieOptions);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error resetting shipping price cookie: {ex.Message}");
			}

			// Điều hướng tới view Checkout
			return View("~/Views/Checkout/Index.cshtml");
		}

		public async Task<IActionResult> Add(long Id)
		{
			ProductModel product = await _dataContext.Products.FindAsync(Id);
			
			List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
			CartItemModel cartItems = cart.Where(c => c.ProductId == Id).FirstOrDefault();
			
			if (cartItems == null)
			{
				cart.Add(new CartItemModel(product));
			}
			else
			{
				cartItems.Quantity += 1;
			}
			HttpContext.Session.SetJson("Cart", cart);

			return Ok(new { success = true, message = "Thêm vào giỏ hàng thành công!" });
		}


		public async Task<IActionResult> Decrease(long Id)
		{
			List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");
			CartItemModel cartItem = cart.FirstOrDefault(c => c.ProductId == Id);
			if (cartItem != null)
			{
				if (cartItem.Quantity > 1)
				{
					cartItem.Quantity--;
				}
				else
				{
					cart.RemoveAll(c => c.ProductId == Id);
				}
				HttpContext.Session.SetJson("Cart", cart);
			}

			return Json(new { success = true, quantity = cartItem?.Quantity });
		}

		public async Task<IActionResult> Increase(long Id)
		{
			ProductModel product = await _dataContext.Products.FirstOrDefaultAsync(p => p.Id == Id);
			List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");
			CartItemModel cartItem = cart.FirstOrDefault(c => c.ProductId == Id);

			if (cartItem != null && product != null)
			{
				if (cartItem.Quantity < product.Quantity)
				{
					cartItem.Quantity++;
				}
				else
				{
					TempData["error"] = "Đã đạt số lượng tối đa của sản phẩm.";
				}
				HttpContext.Session.SetJson("Cart", cart);
			}

			return Json(new { success = true, quantity = cartItem?.Quantity });
		}

		[HttpPost]
		public async Task<IActionResult> Remove(long Id)
		{
			List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");

			cart.RemoveAll(p => p.ProductId == Id);

			if (cart.Count == 0)
			{
				HttpContext.Session.Remove("Cart");
			}
			else
			{
				HttpContext.Session.SetJson("Cart", cart);
			}
			TempData["success"] = "Đã xóa sản phẩm";
			return RedirectToAction("Index");
		}
		public async Task<IActionResult> Clear()
		{
			HttpContext.Session.Remove("Cart");
			TempData["success"] = "Đã xóa tất cả sản phẩm ra giỏ hàng";
			return RedirectToAction("Index");
		}

		[HttpPost]
		[Route("Cart/GetShipping")]
		public async Task<IActionResult> GetShipping(ShippingModel shippingModel, string quan, string tinh, string phuong)
		{
			var existingShipping = await _dataContext.Shippings
			.FirstOrDefaultAsync(x => x.City == tinh && x.District == quan && x.Ward == phuong);
			decimal shippingPrice = 0; // Set mặc định giá tiến
			if (existingShipping != null)
			{
				shippingPrice = existingShipping.Price;
			}
			else
			{
				//Set mặc định giá tiền nếu ko tìm thấy
				shippingPrice = 10000;
			}
			var shippingPriceJson = JsonConvert.SerializeObject(shippingPrice);
			try
			{
				var cookieOptions = new CookieOptions
				{
					HttpOnly = true,
					Expires = DateTimeOffset.UtcNow.AddMinutes(30),
					Secure = true // using HTTPS
				};
				Response.Cookies.Append("ShippingPrice", shippingPriceJson, cookieOptions);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error adding shipping price cookie: {ex.Message}");
			}
			return Json(new { shippingPrice });
		}
		
	}
}
