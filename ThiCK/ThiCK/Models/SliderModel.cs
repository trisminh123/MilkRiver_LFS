using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ThiCK.Repository.Validation;

namespace ThiCK.Models
{
	public class SliderModel
	{
		[Key]
		public long Id { get; set; }
		[Required(ErrorMessage = "Yêu cầu nhập tên slider")]
		public string Name { get; set; }
		[Required(ErrorMessage = "Yêu cầu nhập mô tả slider")]
		public string Description { get; set; }

		public int Status { get; set; }
		public string Image { get; set; }
		[NotMapped]
		[FileExtension]
		public IFormFile? ImageUpload { get; set; }
	}
}
