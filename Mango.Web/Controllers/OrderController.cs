using Mango.Web.Models;
using Mango.Web.Service.IService;
using Mango.Web.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.UserSecrets;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;

namespace Mango.Web.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }
        [Authorize]
        public IActionResult OrderIndex()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAllOrders([FromQuery] string status)
        {
            IEnumerable<OrderHeaderDto> orderList;
            string userId = "";
            if (!User.IsInRole(SD.RoleAdmin))
            {
                userId = User.Claims.Where(c => c.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault()?.Value;
            }
            ResponseDto response = await _orderService.GetAllOrders(userId);
            if (response != null && response.IsSuccess)
            {
                orderList = JsonConvert.DeserializeObject<IEnumerable<OrderHeaderDto>>(Convert.ToString(response.Result));
                switch (status)
                {
                    case "approved":
                        orderList = orderList.Where(o => o.Status == SD.Status_Approved);
                        break;
                    case "readyforpickup":
                        orderList = orderList.Where(o => o.Status == SD.Status_ReadyForPickup);
                        break;
                    case "cancelled":
                        orderList = orderList.Where(o => o.Status == SD.Status_Cancelled || o.Status == SD.Status_Refunded);
                        break;
                    case "completed":
                        orderList = orderList.Where(o => o.Status == SD.Status_Completed);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                orderList = new List<OrderHeaderDto>();
            }
             return Json(new { data = orderList });
        }
  
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> OrderDetail(int orderId)
        {
            OrderHeaderDto order = new();
            string userId = "";
            if (!User.IsInRole(SD.RoleAdmin))
            {
                userId = User.Claims.Where(c => c.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault()?.Value;
            }
            ResponseDto response = await _orderService.GetOrder(orderId);
            if (response != null && response.IsSuccess)
            {
                order = JsonConvert.DeserializeObject<OrderHeaderDto>(Convert.ToString(response.Result));
                if (!User.IsInRole(SD.RoleAdmin) && userId != order.UserId)
                {
                    return NotFound();
                }
            }
            return View(order);
        }

        [HttpPost("OrderReadyForPickup")]
        public async Task<IActionResult> OrderReadyForPickup(int orderId)
        {
            ResponseDto? response = await _orderService.UpdateOrderStatus(orderId, SD.Status_ReadyForPickup);
            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Status updated successfully";
                return RedirectToAction(nameof(OrderDetail),new { orderId = orderId });
            }
            return View();
        }

        [HttpPost("CompleteOrder")]
        public async Task<IActionResult> CompleteOrder(int orderId)
        {
            ResponseDto? response = await _orderService.UpdateOrderStatus(orderId, SD.Status_Completed);
            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Status updated successfully";
                return RedirectToAction(nameof(OrderDetail), new { orderId = orderId });
            }
            return View();
        }

        [HttpPost("CancelOrder")]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            ResponseDto? response = await _orderService.UpdateOrderStatus(orderId, SD.Status_Cancelled);
            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Status updated successfully";
                return RedirectToAction(nameof(OrderDetail), new { orderId = orderId });
            }
            return View();
        }
    }
}
