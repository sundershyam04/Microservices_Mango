using System.ComponentModel.DataAnnotations;

namespace Mango.Services.ProductAPI.Models
{
	public class Product
	{
		[Required]
		public int Id { get; set; }
		[Required]
		public string Name { get; set; } = string.Empty;
		[Required]
		public double Price { get; set; }
		public string Description { get; set; } = string.Empty;
		public string CategoryName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? ImageLocalPath { get; set; }
    }
}
