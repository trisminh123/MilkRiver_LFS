using System.ComponentModel.DataAnnotations;

namespace ThiCK.Models.ViewModels
{
	public class ProductDetailsViewModel
	{
		public ProductModel ProductDetails { get; set; }

		[Required(ErrorMessage = "Vui lòng nhập bình luận sản phẩm")]
		public string Comment { get; set; }
		[Required(ErrorMessage = "Vui lòng nhập tên")]
		public string Name { get; set; }
		[Required(ErrorMessage = "Vui lòng nhập Email")]
		public string Email { get; set; }
		public RatingModel Rating { get; set; }
	}
}
