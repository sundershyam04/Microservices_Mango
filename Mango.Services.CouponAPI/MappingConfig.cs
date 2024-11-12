using AutoMapper;
using Mango.Fro.CouponAPI.Models;
using Mango.Fro.CouponAPI.Models.Dto;

namespace Mango.Fro.CouponAPI
{
    public class MappingConfig
    {
       public static MapperConfiguration RegisterMaps()
        {
            var mappingConfig = new MapperConfiguration(config =>
            {
                config.CreateMap<Coupon, CouponDto>();
                config.CreateMap<CouponDto, Coupon>();
            });
            return mappingConfig;
        }    
    }
}
