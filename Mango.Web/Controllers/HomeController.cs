using Mango.Web.Models;
using Mango.Web.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;

namespace Mango.Web.Controllers
{
    public class HomeController : Controller
    {
        private IProductService _productService;
        private ICartService _cartService;
        public HomeController(IProductService productService, ICartService cartService)
        {
            _productService = productService;
            _cartService = cartService;
        }
        public async Task<IActionResult> Index()
        {
            List<ProductDto>? prodList = null;
            var response = await _productService.GetAllProductsAsync();

            if (response != null && response.IsSuccess)
            {
                prodList = JsonConvert.DeserializeObject<List<ProductDto>>(Convert.ToString(response.Result));
            }
            else
            {
                TempData["error"] = response?.Message;
            }
            return View(prodList);
        }

        [Authorize]
        public async Task<IActionResult> Details(int productId)
        {
            ProductDto? product = null;
            var response = await _productService.GetProductbyIdAsync(productId);
            if (response != null && response.IsSuccess)
            {
                product = JsonConvert.DeserializeObject<ProductDto>(Convert.ToString(response.Result));
            }
            else
            {
                TempData["error"] = response?.Message;
            }
            return View(product);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Details(ProductDto productDto)
        {
            CartDto cart = new()
            {
                CartHeader = new()
                {
                    UserId = User.Claims.Where(t => t.Type == JwtRegisteredClaimNames.Sub).FirstOrDefault()?.Value
                }
            };

            CartDetailDto cartDetail = new()
            {
                ProductId = productDto.Id,
                Count = productDto.Count,
            };

            List<CartDetailDto> cartDetails = new() { cartDetail};
            cart.CartDetails = cartDetails;
            ResponseDto? response = await _cartService.UpsertCartAsync(cart);

            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Product Added to Cart";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["error"] = response?.Message;
            }
            return View(productDto);
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
