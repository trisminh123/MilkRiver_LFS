using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ThiCK.Repository.Validation;
namespace ThiCK.Models
{
	public class ProductModel
	{
		[Key]
		public long Id { get; set; }
		[Required(ErrorMessage = "Yêu cầu nhập tên sản phẩm")]
		public string Name { get; set; }
		public string Slug { get; set; }
		[Required(ErrorMessage = "Yêu cầu nhập mô tả sản phẩm")]
		public string Description { get; set; }
		[Required(ErrorMessage = "Yêu cầu nhập giá sản phẩm")]
		[Range(0.01, double.MaxValue)] 
		
		[Column(TypeName = "decimal(18,3)")]
		public decimal Price { get; set; }
		public int Quantity { get; set; } = 0;
		public int Sold { get; set; } = 0;

		[Required(ErrorMessage = "Chọn một thương hiệu")]
		public long BrandId { get; set; }
		[Required, Range(1, int.MaxValue, ErrorMessage = "Chọn một danh mục")]
		public long CategoryId { get; set; }
		[ForeignKey("CategoryId")]
		public CategoryModel Category { get; set; }
		[ForeignKey("BrandId")]
		public BrandModel Brand { get; set; }
		public string Image { get; set; }
		public ICollection<RatingModel> Ratings { get; set; }
		[NotMapped]
		[FileExtension]
		public IFormFile? ImageUpload {  get; set; }
	}
}
