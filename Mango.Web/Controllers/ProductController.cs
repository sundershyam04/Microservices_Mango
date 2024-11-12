using Mango.Web.Models;
using Mango.Web.Service.IService;
using Mango.Web.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Mango.Web.Controllers
{

	public class ProductController : Controller
	{
		private readonly IProductService _productService;
		public ProductController(IProductService productService)
		{
			_productService = productService;
		}

		public async Task<IActionResult?> ProductIndex()
		{
			List<ProductDto>? productList = null;
			ResponseDto? result = await _productService.GetAllProductsAsync();

			if (result != null && result.IsSuccess)
			{
				productList = JsonConvert.DeserializeObject<List<ProductDto>>(Convert.ToString(result.Result));
			}
			else
			{
				TempData["error"] = result?.Message;
			}
			return View(productList);
		}
		public IActionResult ProductCreate()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> ProductCreate(ProductDto productDto)
		{
			ResponseDto? result = await _productService.CreateProductAsync(productDto);

			if (result != null && result.IsSuccess)
			{
				TempData["success"] = "Product created!";
				return RedirectToAction(nameof(ProductIndex));
			}
			else
			{
				TempData["error"] = result?.Message;
			}
			return View();
		}
		public async Task<IActionResult> ProductEdit(int productId)
		{
			ProductDto? product = null;
			ResponseDto? result = await _productService.GetProductbyIdAsync(productId);

			if (result != null && result.IsSuccess)
			{
				product = JsonConvert.DeserializeObject<ProductDto>(Convert.ToString(result.Result));
				return View(product);
			}
			else
			{
				TempData["error"] = result?.Message;
			}
			return NotFound();

		}
		[HttpPost]
		public async Task<IActionResult> ProductEdit(ProductDto productDto)
		{
			ResponseDto? result = await _productService.UpdateProductAsync(productDto);

			if (result != null && result.IsSuccess)
			{
				TempData["success"] = "Product Edited successfully";
				return RedirectToAction(nameof(ProductIndex));
			}
			else
			{
				TempData["error"] = result?.Message;
			}
			return View(productDto);
		}

		public async Task<IActionResult> ProductDelete(int productId)
		{
			ProductDto? product = null;
			ResponseDto? result = await _productService.GetProductbyIdAsync(productId);

			if (result != null && result.IsSuccess)
			{
				product = JsonConvert.DeserializeObject<ProductDto>(Convert.ToString(result.Result));
				return View(product);
			}
			else
			{
				TempData["error"] = result?.Message;
			}
			return NotFound();

		}
		[HttpPost]
		public async Task<IActionResult> ProductDelete(ProductDto productDto)
		{
			ResponseDto? result = await _productService.DeleteProductAsync(productDto.Id);

			if (result != null && result.IsSuccess)
			{
				TempData["success"] = "Product Deleted successfully";
				return RedirectToAction(nameof(ProductIndex));
			}
			else
			{
				TempData["error"] = result?.Message;
			}
			return View(productDto);
		}
       
    }
}
