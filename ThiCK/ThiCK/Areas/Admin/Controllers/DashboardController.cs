using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using ThiCK.Repository;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ThiCK.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Route("Admin/Dashboard")]
	[Authorize(Roles = "Admin,SalesManager,Employee")]
	public class DashboardController : Controller
	{

		private readonly DataContext _dataContext;
		private readonly IWebHostEnvironment _webHostEnvironment;
		public DashboardController(DataContext context, IWebHostEnvironment webHostEnvironment)
		{
			_dataContext = context;
			_webHostEnvironment = webHostEnvironment;
		}
		public IActionResult Index()
		{
			var count_product = _dataContext.Products.Count();
			var count_order = _dataContext.Orders.Count();
			var count_category = _dataContext.Categories.Count();
			var count_user = _dataContext.Users.Count();
			ViewBag.CountProduct = count_product;
			ViewBag.CountOrder = count_order;
			ViewBag.CountCategory = count_category;
			ViewBag.CountUser = count_user;
			return View();
		}
		[HttpPost]
		[Route("GetChartData")]
		public async Task<IActionResult> GetChartData()
		{
			var data = _dataContext.Statisticals
			.Select(s => new
			{
				date = s.DateCreated.ToString("yyyy-MM-dd"),
				sold = s.Sold,
				quantity = s.Quantity,
				revenue = s.Revenue,
				profit = s.Profit
			})
			.ToList();
			return Json(data);
		}
		[HttpPost]
		[Route("GetChartDataBySelect")]
		public IActionResult GetChartDataBySelect(DateTime startDate, DateTime endDate)
		{
			var data = _dataContext.Statisticals
			.Where(s => s.DateCreated >= startDate && s.DateCreated <= endDate)
			.GroupBy(s => s.DateCreated.Date)
			.Select(g => new
			{
				date = g.Key.ToString("yyyy-MM-dd"),
				sold = g.Sum(s => s.Sold),
				quantity = g.Sum(s => s.Quantity),
				revenue = g.Sum(s => s.Revenue),
				profit = g.Sum(s => s.Profit)
			})
			.ToList();
			return Json(data);
		}
		[HttpPost]
		[Route("FilterData")]
		public IActionResult FilterData(DateTime? fromDate, DateTime? toDate)
		{
			var query = _dataContext.Statisticals.AsQueryable();
			if (fromDate.HasValue)
			{
				query = query.Where(s => s.DateCreated >= fromDate);
			}
			if (toDate.HasValue)
			{
				query = query.Where(s => s.DateCreated <= toDate);
			}
			var data = query
			.Select(s => new
			{
				date = s.DateCreated.ToString("yyyy-MM-dd"),
				sold = s.Sold,
				quantity = s.Quantity,
				revenue = s.Revenue,
				profit = s.Profit
			})
			.ToList();
			return Json(data);
		}
	}
}
