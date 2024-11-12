using Mango.Services.ShoppingCartAPI.Models.Dto;
using Mango.Services.ShoppingCartAPI.Service.IService;
using Newtonsoft.Json;

namespace Mango.Services.ShoppingCartAPI.Service
{
    public class CouponService:ICouponService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public CouponService(IHttpClientFactory httpClientFactory) 
        { 
            _httpClientFactory = httpClientFactory;
        }

        public async Task<CouponDto> GetCoupon(string couponCode)
        {          
                var client = _httpClientFactory.CreateClient("Coupon");
                var response = await client.GetAsync($"/api/coupon/GetByCode/{couponCode}");
                var responsestr = await response.Content.ReadAsStringAsync();
                var responseDto = JsonConvert.DeserializeObject<ResponseDto>(responsestr);

                if (responseDto!=null && responseDto.Result != null && responseDto.IsSuccess)
                {
                    return JsonConvert.DeserializeObject<CouponDto>(Convert.ToString(responseDto.Result));
                }
                return new CouponDto();
        }
    }
}
