using Mango.Web.Models;
using Mango.Web.Service.IService;
using Mango.Web.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Mango.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;

        public CartController(ICartService cartService, IOrderService orderService)
        {
            _cartService = cartService;
            _orderService = orderService;
        }

        [Authorize]
        public async Task<IActionResult> CartIndex()
        {
            return View(await LoadCartBasedonLoggedInUser());
        }

        [Authorize]
        public async Task<IActionResult> Checkout()
        {                       
            return View(await LoadCartBasedonLoggedInUser());
        }

        /*
         * Create OrderHeader and Stripe Session with 2 separate endpoints in OrderAPI
         */
        [Authorize]
        [HttpPost]
        [ActionName("Checkout")]       
        public async Task<IActionResult> Checkout(CartDto cartDto)
        {
            CartDto cart = await LoadCartBasedonLoggedInUser();
            try
            {             
                cart.CartHeader.Email = cartDto.CartHeader.Email;
                cart.CartHeader.Phone = cartDto.CartHeader.Phone;
                cart.CartHeader.Name = cartDto.CartHeader.Name;

                ResponseDto? response = await _orderService.CreateOrder(cart);
                OrderHeaderDto? orderHeaderDto = JsonConvert.DeserializeObject<OrderHeaderDto>(Convert.ToString(response.Result));

                if (response != null && response.IsSuccess)
                {
                    //stripe integration
                    var domain = Request.Scheme + "://" + Request.Host.Value + "/";

                    StripeRequestDto stripeRequest = new()
                    {
                        ApprovedUrl = domain + "Cart/Confirmation?orderId=" + orderHeaderDto.OrderHeaderId,
                        CancelUrl = domain + "Cart/Checkout",
                        OrderHeader = orderHeaderDto
                    };

                    ResponseDto? stripeResponse = await _orderService.CreateStripeSession(stripeRequest);
                    var stripeResponseResult = JsonConvert.DeserializeObject<StripeRequestDto>(Convert.ToString(stripeResponse.Result));

                    Response.Headers.Location = stripeResponseResult.StripeSessionUrl;
                    return new StatusCodeResult(303);
                }   

            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }
           return View(cart);
        }

        [Authorize]
        public async Task<IActionResult> Confirmation(int orderId)
        {
            ResponseDto? response = await _orderService.ValidateStripeSession(orderId);
            var orderHeader = JsonConvert.DeserializeObject<OrderHeaderDto>(Convert.ToString(response.Result));
            if(orderHeader.Status == SD.Status_Approved)
            {
                return View(orderId); // payment success --> Display Confirmation Page
            }
            // can return an Error Page with info
            return View(orderId);
        }


        [Authorize]      
        public async Task<IActionResult> RemoveCartItem(int cartDetailId)
        {
            ResponseDto? response;
            try  
            {
               response = await _cartService.RemoveFromCartAsync(cartDetailId);
                if (response != null && response.IsSuccess)
                {
                    TempData["success"] = "Item removed from cart!";          
                }
                else
                {
                    TempData["error"] = response?.Message;
                }
            }
           catch(Exception ex)
            {
                TempData["error"] = ex.Message;
            }
            return RedirectToAction(nameof(CartIndex));

        }

        [HttpPost]
        public async Task<IActionResult> ApplyCoupon(CartDto cart)
        {
            ResponseDto response = await _cartService.ApplyCouponAsync(cart);

            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Coupon applied!";
                return RedirectToAction(nameof(CartIndex));
            }
            else
            {
                TempData["error"] = response?.Message;
            }
            return RedirectToAction(nameof(CartIndex));
        }

        [HttpPost]
        public async Task<IActionResult> RemoveCoupon(CartDto cart)
        {
            cart.CartHeader.CouponCode = "";
            ResponseDto response = await _cartService.ApplyCouponAsync(cart);

            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Coupon Removed!";
                return RedirectToAction(nameof(CartIndex));
            }
            else
            {
                TempData["error"] = response?.Message;
            }
            return RedirectToAction(nameof(CartIndex));

        }

        [HttpPost]
        public async Task<IActionResult> EmailCart(CartDto cartDto)
        {
            CartDto cart = await LoadCartBasedonLoggedInUser();
            cart.CartHeader.Email = User.Claims.Where(c => c.Type == JwtRegisteredClaimNames.Email)?.FirstOrDefault()?.Value;
            var response = await _cartService.EmailCartAsync(cart);
            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Email will be sent shortly!";
                return RedirectToAction(nameof(CartIndex));
            }
            else
            {
                TempData["error"] = response?.Message;
                return RedirectToAction(nameof(CartIndex));
            }
            
        }

        private async Task<CartDto> LoadCartBasedonLoggedInUser()
        {
            var userId = User.Claims.Where(t => t.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault()?.Value;

            ResponseDto? response = await _cartService.GetCartByUserIdAsync(userId);

            if (response != null && response.IsSuccess)
            {
                var cart = JsonConvert.DeserializeObject<CartDto>(Convert.ToString(response.Result));
                //cart.CartHeader.Email = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Email)?.FirstOrDefault()?.Value;
                return cart;
            }
            return new CartDto();
        }
    }
}
 