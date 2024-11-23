using System.ComponentModel.DataAnnotations;

namespace Mango.Web.Utility
{
    public class AllowedExtensionsAttribute : ValidationAttribute
    {
        private readonly string[] _allowedExtensions;
        public AllowedExtensionsAttribute(string[] extensions) 
        {
            _allowedExtensions = extensions;
        }

        // [value]: FormFile type (Image)
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var file = value as IFormFile;
            if (file != null)
            {
                var extension = Path.GetExtension(file.FileName);
                if (!_allowedExtensions.Contains(extension))
                {
                    return new ValidationResult("This photo extension is not allowed");
                }
            }
            return ValidationResult.Success;
        }
    }
}
