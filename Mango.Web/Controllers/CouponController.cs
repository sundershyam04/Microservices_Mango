using Mango.Web.Models;
using Mango.Web.Service.IService;
using Mango.Web.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Mango.Web.Controllers
{
    public class CouponController : Controller
    {
        private readonly ICouponService _couponService;
        public CouponController(ICouponService couponService)
        {
            _couponService = couponService;
        }
       
        public async Task<IActionResult?> CouponIndex()
        {
            List<CouponDto>? couponList = null;                     
            ResponseDto? result = await _couponService.GetAllCouponsAsync();

            if (result != null && result.IsSuccess)
            {
                couponList = JsonConvert.DeserializeObject<List<CouponDto>>(Convert.ToString(result.Result));
            }
            else
            {
                TempData["error"] = result?.Message;
            }
            return View(couponList);
        }
        public IActionResult CouponCreate()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CouponCreate(CouponDto couponDto)
        {
            ResponseDto? result = await _couponService.CreateCouponAsync(couponDto);

            if (result != null && result.IsSuccess)
            {
                TempData["success"] = "Coupon created!";
                return RedirectToAction(nameof(CouponIndex));             
            }
            else
            {
                TempData["error"] = result?.Message;
            }
            return View();
        }

        public async Task<IActionResult> CouponDelete(int couponId)
        {
           CouponDto? coupon = null;
           ResponseDto? result = await _couponService.GetCouponbyIdAsync(couponId);

            if(result != null && result.IsSuccess)
            {
				coupon = JsonConvert.DeserializeObject<CouponDto>(Convert.ToString(result.Result));
                return View(coupon);
            }
            else
            {
                TempData["error"] = result?.Message;
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> CouponDelete(CouponDto couponDto)
        {
            ResponseDto? result = await _couponService.DeleteCouponAsync(couponDto.CouponId);

            if (result != null && result.IsSuccess)
            {
                TempData["success"] = "Coupon Deleted successfully";
                return RedirectToAction(nameof(CouponIndex));
            }
            else
            {
                TempData["error"] = result?.Message;
            }
            return View(couponDto);
        }
    }
}
