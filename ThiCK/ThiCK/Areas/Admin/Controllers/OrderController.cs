using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System.Security.Claims;
using ThiCK.Models;
using ThiCK.Repository;

namespace ThiCK.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Route("Admin/Order")]
	[Authorize(Roles = "Admin,SalesManager,Employee")]
	public class OrderController : Controller
	{
        private readonly DataContext _dataContext;

		private readonly ThiCK.Areas.Admin.Repository.IEmailSender _emailSender;

		public OrderController(DataContext context, ThiCK.Areas.Admin.Repository.IEmailSender emailSender)
		{
			_dataContext = context;
			_emailSender = emailSender;
		}
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
			List<OrderModel> order = _dataContext.Orders.OrderByDescending(o => o.CreatedDate).ToList();



			return View(order);
        }
        [Route("ViewOrder")]
		public async Task<IActionResult> ViewOrder(string ordercode)
		{
			var DetailsOrder = await _dataContext.OrderDetails.Include(od=>od.Product).Where(od=>od.OrderCode==ordercode).ToListAsync();
			var Order = _dataContext.Orders.Where(o => o.OrderCode == ordercode).First();
			ViewBag.ShippingCost = Order.ShippingCost;
            ViewBag.Status = Order.Status;
			return View(DetailsOrder);
		}
        [HttpPost]
        [Route("UpdateOrder")]
        public async Task<IActionResult> UpdateOrder(string ordercode, int status)
        {
            var order = await _dataContext.Orders.FirstOrDefaultAsync(o => o.OrderCode == ordercode);

            if (order == null)
            {
                return NotFound();
            }
            order.Status = status;
            _dataContext.Update(order);

            if (status == 2) // khi giao hang thanh cong moi thong ke
            {
				var userEmail = User.FindFirstValue(ClaimTypes.Email);
				var receiver = userEmail;
				var subject = "Giao hàng thành công";
				var message = "Giao hàng thành công, cảm ơn quý khách đã ủng hộ shop Milk River";

				await _emailSender.SendEmailAsync(receiver, subject, message);
				// lấy dữ liệu order detail dựa vào order. OrderCode
				var DetailsOrder = await _dataContext.OrderDetails
                .Include(od => od.Product)
                .Where(od => od.OrderCode == order.OrderCode)
                .Select(od => new
                {
                    od.Quantity,
                    od.Product.Price,
                }).ToListAsync();
                // lấy data thống kê dựa vào ngày đặt hàng
                var statisticalModel = await _dataContext.Statisticals
                .FirstOrDefaultAsync(s => s.DateCreated.Date == order.CreatedDate.Date);
                if (statisticalModel != null)
                {
                    foreach (var orderDetail in DetailsOrder)
                    {
                        // tổn tại ngày thì cộng dồn
                        statisticalModel.Quantity += 1;
                        statisticalModel.Sold += orderDetail.Quantity;
                        statisticalModel.Revenue += orderDetail.Quantity * orderDetail.Price;
                        statisticalModel.Profit += statisticalModel.Revenue * 20 / 100; // loi 20% so voi doanh thu
                    }
                    _dataContext.Update(statisticalModel);
                }
                else
                {
                    int new_quantity = 0;
                    int new_sold = 0;
                    decimal new_profit = 0;
                    foreach (var orderDetail in DetailsOrder)
                    {
                        new_quantity += 1;
                        new_sold += orderDetail.Quantity;
                        new_profit += orderDetail.Price * 20 / 100;
                        statisticalModel = new StatisticalModel
                        {
                            DateCreated = order.CreatedDate,
                            Quantity = new_quantity,
                            Sold = new_sold,
                            Revenue = orderDetail.Quantity * orderDetail.Price,
                            Profit = new_profit
                        };
                    }
                    _dataContext.Add(statisticalModel);
                }
            }
            try
            {
                await _dataContext.SaveChangesAsync();
                return Ok(new {success = true, message = "Order status updated successfully"});
            }
            catch (Exception ex)
            {

                return StatusCode(500, "An error occurred while updating the order status");
            }
        }
		
		[Route("DeleteOrder")]
		public async Task<IActionResult> DeleteOrder(string ordercode)
		{
			// Tìm đơn hàng theo mã đơn hàng
			var order = await _dataContext.Orders.FirstOrDefaultAsync(o => o.OrderCode == ordercode);

			if (order == null)
			{
                TempData["error"] = "Không tìm thấy đơn hàng";
				return RedirectToAction("Index");

			}

			try
			{
				// Tìm tất cả các chi tiết đơn hàng liên quan
				var orderDetails = await _dataContext.OrderDetails
					.Where(od => od.OrderCode == ordercode)
					.ToListAsync();

				// Xóa chi tiết đơn hàng
				_dataContext.OrderDetails.RemoveRange(orderDetails);

				// Xóa đơn hàng
				_dataContext.Orders.Remove(order);

				// Lưu thay đổi vào cơ sở dữ liệu
				await _dataContext.SaveChangesAsync();
                TempData["success"] = "Đã xóa đơn hàng thành công";
				return RedirectToAction("Index");
				;
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { success = false, message = "An error occurred while deleting the order", error = ex.Message });
			}
		}

	}
}
