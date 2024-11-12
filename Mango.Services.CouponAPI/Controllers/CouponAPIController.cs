using AutoMapper;
using Azure;
using Mango.Fro.CouponAPI.Data;
using Mango.Fro.CouponAPI.Models;
using Mango.Fro.CouponAPI.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;


namespace Mango.Fro.CouponAPI.Controllers
{
    [Route("api/coupon")]
    [ApiController]
    [Authorize]
    public class CouponAPIController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ResponseDto _response;
        private readonly IMapper _mapper;


        public CouponAPIController(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
            _response = new ResponseDto();
        }

        [HttpGet]
        public ResponseDto Get()
        {
            try
            {
                IEnumerable<Coupon> objList = _db.Coupons.ToList();
                _response.Result = _mapper.Map<IEnumerable<CouponDto>>(objList);
                return _response;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                _response.Result = null;
                return _response;
            }
            
        }

        [HttpGet]   
        [Route("{id}")]
        public ResponseDto Get(int id)   
        {
            try
            {
                Coupon obj = _db.Coupons.First<Coupon>( res => res.CouponId == id);               
                _response.Result = _mapper.Map<CouponDto>(obj);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            } 
            return _response;
        }

        [HttpGet]
        [Route("GetByCode/{code}")]
        public ResponseDto Get(string code)
        {
            try
            {
                Coupon obj = _db.Coupons.First(res => res.CouponCode.ToLower() == code.ToLower());
                // FirstOrDefault - returns null if no objects match
                // if(obj == null) { _response.IsSuccess = false; }             
                _response.Result = _mapper.Map<CouponDto>(obj);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }
        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public ResponseDto Post([FromBody] CouponDto dtoObj) 
        {   
            try
            {
                Coupon obj = _mapper.Map<Coupon>(dtoObj);
                _db.Coupons.Add(obj);
                _db.SaveChanges();

                // stripe - post coupons
                var options = new Stripe.CouponCreateOptions()
                { 
                    AmountOff = (long)(dtoObj.DiscountAmount * 100),
                    Name = dtoObj.CouponCode,
                    Currency = "usd",
                    Id= dtoObj.CouponCode,                    
                };

                var service = new Stripe.CouponService();
                service.Create(options);
                //stripe coupon - created

                _response.Result = _mapper.Map<CouponDto>(obj);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        /// <summary>
        /// If CouponId present in coupon tbl, update happens
        /// If Id=0,new records added in tbl
        /// If Id out of bound( ID : not present in tbl and non-zero), exception thrown DbUpdateConcurrencyException
        /// </summary>
        /// <param name="dtoObj"></param>
        /// <returns></returns>

        [HttpPut]
		[Authorize(Roles = "ADMIN")]
		public ResponseDto Put([FromBody] CouponDto dtoObj) 
        {
            try
            {
                Coupon obj = _mapper.Map<Coupon>(dtoObj);
                _db.Coupons.Update(obj);
                _db.SaveChanges();
                _response.Result = _mapper.Map<CouponDto>(obj);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }
        [HttpDelete]
        [Route("{id}")]
		[Authorize(Roles = "ADMIN")]
		public ResponseDto Delete(int id)
        {
            try
            {
                Coupon obj =  _db.Coupons.First(item => item.CouponId == id);
                _db.Coupons.Remove(obj);
                _db.SaveChanges();
                // stripe -coupon delete
                var service = new Stripe.CouponService();
                service.Delete(obj.CouponCode);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }
    }
    }
