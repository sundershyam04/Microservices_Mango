﻿
using Mango.Web.Utility;
using System.ComponentModel.DataAnnotations;

namespace Mango.Web.Models
{
	public class ProductDto
	{
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public double Price { get; set; }
		public string Description { get; set; } = string.Empty;
		public string CategoryName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? ImageLocalPath { get; set; }

		[MaxFileSize(1)]
        [AllowedExtensions([".jpg", ".png",".jpeg"])]
        public IFormFile? Image { get; set; }
        [Range(1,100)]
		public int Count { get; set; } = 1;
	}
}
