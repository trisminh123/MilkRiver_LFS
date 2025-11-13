using Microsoft.AspNetCore.Mvc;
using ThiCK.Models;
using System.Security.Claims;
using ThiCK.Repository;
using Microsoft.EntityFrameworkCore;
using ThiCK.Areas.Admin.Repository;
using Newtonsoft.Json;

namespace ThiCK.Controllers
{
    public class CheckoutController : Controller
	{
		private readonly DataContext _dataContext;
		private readonly IEmailSender _emailSender;

		public CheckoutController(IEmailSender emailSender, DataContext context)
		{
			_dataContext = context;
			_emailSender = emailSender;
		}
		public IActionResult Index()
		{
			return View();
		}
		public async Task<IActionResult> Checkout()
		{
			var userEmail = User.FindFirstValue(ClaimTypes.Email);
			if(userEmail == null)
			{
				return RedirectToAction("Login", "Account");
			}
			else
			{
				var ordercode = Guid.NewGuid().ToString();
				var orderItem = new OrderModel();


				// Nhận giá shipping từ cookie
				var shippingPriceCookie = Request.Cookies["ShippingPrice"];
				decimal shippingPrice = 0;
				if (shippingPriceCookie != null)
				{
					var shippingPriceJson = shippingPriceCookie;
					shippingPrice = JsonConvert.DeserializeObject<decimal>(shippingPriceJson);
				}

				orderItem.OrderCode = ordercode;
				orderItem.ShippingCost = shippingPrice;
				orderItem.UserName = userEmail;
				orderItem.Status = 1;
				orderItem.CreatedDate = DateTime.Now;

				_dataContext.Add(orderItem);
				_dataContext.SaveChanges();
				List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
				foreach(var cart in cartItems)
				{
					var orderdetails = new OrderDetails();
					orderdetails.UserName = userEmail;
					orderdetails.OrderCode = ordercode;
					orderdetails.Price = cart.Price;
					orderdetails.ProductId = cart.ProductId;
					orderdetails.Quantity = cart.Quantity;

					var product = await _dataContext.Products.Where(p => p.Id == cart.ProductId).FirstAsync();
					product.Quantity -= cart.Quantity;
					product.Sold += cart.Quantity;

					

					// Thêm bản ghi mới vào ProductQuantities
					var productQuantityUpdate = new ProductQuantityModel
					{
						ProductId = cart.ProductId,
						Quantity = -cart.Quantity, // Số lượng giảm
						DateCreated = DateTime.Now
					};
					_dataContext.Add(productQuantityUpdate);

					_dataContext.Update(product);
					_dataContext.Add(orderdetails);
					_dataContext.SaveChanges();

				}
				HttpContext.Session.Remove("Cart");
				// gui mail thong bao
				var receiver = userEmail;
				var subject = "Đặt hàng thành công tại shop Milk River";
				var message = "Đặt hàng thành công, trải nghiệm dịch vụ nhé";

				await _emailSender.SendEmailAsync(receiver, subject, message);
				TempData["success"] = "Đặt hàng thành công, chờ duyệt đơn hàng";
				return RedirectToAction("History", "Account");
			}
			return View();
		}
	}
}
