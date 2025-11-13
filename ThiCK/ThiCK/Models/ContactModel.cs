using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ThiCK.Repository.Validation;

namespace ThiCK.Models
{
	public class ContactModel
	{
		[Key]
		[Required(ErrorMessage = "Yêu cầu nhập tiêu đề website")]
		public string Name { get; set; }
		[Required(ErrorMessage = "Yêu cầu nhập bản đồ")]
		public string Map { get; set; }
		[Required(ErrorMessage = "Yêu cầu nhập email liên hệ")]
		public string Email { get; set; }
		[Required(ErrorMessage = "Yêu cầu nhập số điện thoại liên hệ")]
		public string Phone { get; set; }
		[Required(ErrorMessage = "Yêu cầu nhập thông tin liên hệ")]
		public string Description { get; set; }
		public string LogoImg { get; set; }
		[NotMapped]
		[FileExtension]
		public IFormFile? ImageUpload { get; set; }
	}
}
