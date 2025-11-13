using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Globalization;
using ThiCK.Models;
using ThiCK.Models.ViewModels;
using ThiCK.Repository;

namespace ThiCK.Controllers
{
    public class HomeController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<AppUserModel> _userManager;

        public HomeController(ILogger<HomeController> logger, DataContext context, UserManager<AppUserModel> userManager)
        {
            _logger = logger;
            _dataContext = context;
            _userManager = userManager;
        }

		public async Task<IActionResult> Index(string sort_by = "", string startprice = "", string endprice = "", int page = 1)
		{
			IQueryable<ProductModel> products = _dataContext.Products.Include("Category").Include("Brand");
			var sliders = _dataContext.Sliders.Where(s => s.Status == 1).ToList();
			ViewBag.Sliders = sliders;

			if (!string.IsNullOrEmpty(sort_by))
			{
				if (sort_by == "price_increase")
					products = products.OrderBy(p => p.Price);
				else if (sort_by == "price_decrease")
					products = products.OrderByDescending(p => p.Price);
				else if (sort_by == "price_newest")
					products = products.OrderByDescending(p => p.Id);
				else if (sort_by == "price_oldest")
					products = products.OrderBy(p => p.Id);
			}

			if (!string.IsNullOrEmpty(startprice) && !string.IsNullOrEmpty(endprice))
			{
				if (decimal.TryParse(startprice, out decimal startPriceValue) && decimal.TryParse(endprice, out decimal endPriceValue))
				{
					products = products.Where(p => p.Price >= startPriceValue && p.Price <= endPriceValue);
				}
			}

			int totalItems = await products.CountAsync();
			const int pageSize = 9; // Số sản phẩm mỗi trang
			var paginate = new Paginate(totalItems, page, pageSize);

			var pagedProducts = await products
				.Skip((paginate.CurrentPage - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			ViewBag.Paginate = paginate;

			var user = await _userManager.GetUserAsync(User);
			// Lấy danh sách sản phẩm yêu thích của người dùng
			var wishlistProductIds = user != null
				? await _dataContext.Wishlists
					.Where(w => w.UserId == user.Id)
					.Select(w => w.ProductId)
					.ToListAsync()
				: new List<long>();

			// Lấy danh sách sản phẩm yêu thích của người dùng
			var compareProductIds = user != null
				? await _dataContext.Compares
					.Where(w => w.UserId == user.Id)
					.Select(w => w.ProductId)
					.ToListAsync()
				: new List<long>();

			// Kết hợp thông tin sản phẩm với trạng thái yêu thích
			var viewModel = pagedProducts.Select(product => new ProductWishlistCompareViewModel
			{
				Product = product,
				IsInWishlist = wishlistProductIds.Contains(product.Id),
				IsInCompare = compareProductIds.Contains(product.Id)
			}).ToList();

			return View(viewModel);
		}


		public async Task<IActionResult> Contact()
        {
            var contact = await _dataContext.Contacts.FirstAsync();
            return View(contact);
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int statuscode)
        {
            if (statuscode == 404)
            {
                return View("NotFound");
            }
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public async Task<IActionResult> Wishlist()
        { 


            var wishlist_product = await(from w in _dataContext.Wishlists

                                         join p in _dataContext.Products on w.ProductId equals p.Id

                                         join u in _dataContext.Users on w.UserId equals u.Id
                                         select new WishlistViewModel{ User = u, Product = p, Wishlists = w })
                                         .ToListAsync();
            return View(wishlist_product);
    }

		public async Task<IActionResult> AddWishList(long Id)
        {
            var user = await _userManager.GetUserAsync(User);

			var wishList = new WishlistModel
			{
				ProductId = Id,
				UserId = user.Id,
            };
            _dataContext.Add(wishList);



			try
			{
                await _dataContext.SaveChangesAsync();
                return Ok(new { success = true, message = "Thêm yêu thích thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while adding to wishlist table");
            }

		}
		
		public async Task<IActionResult> DeleteWishlist(long Id)
		{
			WishlistModel wishlist = await _dataContext.Wishlists.FindAsync(Id);

			_dataContext.Wishlists.Remove(wishlist);
			await _dataContext.SaveChangesAsync();
			TempData["success"] = "Đã xóa yêu thích thành công";
            return RedirectToAction("Wishlist", "Home");
		}
		[HttpPost]
		public async Task<IActionResult> DeleteWishlistIndex(long id)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null) return Json(new { success = false, message = "User not authenticated." });

			var wishlistItem = await _dataContext.Wishlists.FirstOrDefaultAsync(w => w.ProductId == id && w.UserId == user.Id);
			if (wishlistItem == null) return Json(new { success = false, message = "Item not found in wishlist." });

			_dataContext.Wishlists.Remove(wishlistItem);
			await _dataContext.SaveChangesAsync();

			return Json(new { success = true });
		}

		
		public async Task<IActionResult> Compare()
		{
			var compare_product = await (from c in _dataContext.Compares
										 join p in _dataContext.Products on c.ProductId equals p.Id
										 join u in _dataContext.Users on c.UserId equals u.Id
										 select new CompareViewModel
										 {
											 User = u,
											 Product = p,
											 Compares = c
										 }).ToListAsync();

			return View(compare_product);
		}

		public async Task<IActionResult> AddCompare(long Id)
		{
			var user = await _userManager.GetUserAsync(User);

			var compareProduct = new CompareModel
			{
				ProductId = Id,
				UserId = user.Id,
            };

		    _dataContext.Compares.Add(compareProduct);

			try
			{
				await _dataContext.SaveChangesAsync();
				return Ok(new { success = true, message = "Thêm so sánh sản phẩm thành công" });
			}
			catch (Exception ex)
			{
				return StatusCode(500, "An error occurred while adding to compare table");
			}

		}
		public async Task<IActionResult> DeleteCompare(long Id)
		{
			CompareModel compare = await _dataContext.Compares.FindAsync(Id);

			_dataContext.Compares.Remove(compare);
			await _dataContext.SaveChangesAsync();
			TempData["success"] = "Đã xóa so sánh thành công";
            return RedirectToAction("Compare", "Home");
		}
		[HttpPost]
		public async Task<IActionResult> DeleteCompareIndex(long id)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null) return Json(new { success = false, message = "User not authenticated." });

			var compareItem = await _dataContext.Compares.FirstOrDefaultAsync(w => w.ProductId == id && w.UserId == user.Id);
			if (compareItem == null) return Json(new { success = false, message = "Item not found in wishlist." });

			_dataContext.Compares.Remove(compareItem);
			await _dataContext.SaveChangesAsync();

			return Json(new { success = true });
		}
	}
}
