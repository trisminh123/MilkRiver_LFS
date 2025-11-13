using System.ComponentModel.DataAnnotations;

namespace ThiCK.Repository.Validation
{
	public class FileExtensionAttribute : ValidationAttribute
	{
		protected override ValidationResult? IsValid(object  value, ValidationContext validationContext)
		{
			if(value is IFormFile file)
			{
				var extention = Path.GetExtension(file.FileName).ToLowerInvariant();
				string[] extensions = { "jpg", "png", "jpeg" ,"gif", "bmp", "webp" };

				bool result = extensions.Any(x => extention.EndsWith(x));

				if(!result)
				{
					return new ValidationResult("Allowed extensions are jpg, png, jpeg, gif, bmp, webp");
				}
			}
			return ValidationResult.Success;
		}
	}
}
