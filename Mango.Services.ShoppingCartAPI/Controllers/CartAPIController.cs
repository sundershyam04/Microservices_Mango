using AutoMapper;
using Mango.Messaging;
using Mango.Services.ShoppingCartAPI.Data;
using Mango.Services.ShoppingCartAPI.Models;
using Mango.Services.ShoppingCartAPI.Models.Dto;
using Mango.Services.ShoppingCartAPI.Service.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;

namespace Mango.Services.ShoppingCartAPI.Controllers
{
    [Route("api/cart")]
    [ApiController]
    public class CartAPIController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        private readonly ResponseDto _response;
        private readonly IProductService _productService;
        private readonly ICouponService _couponService;
        private readonly IMessageBus _messageBus;
        private readonly IConfiguration _configuration;
        public CartAPIController(AppDbContext db, IMapper mapper, IProductService productService, ICouponService couponService, IMessageBus messageBus, IConfiguration configuration)
        {
            _db = db;
            _mapper = mapper;
            this._response = new ResponseDto();
            _productService = productService;
            _couponService = couponService;
            _messageBus = messageBus;
            _configuration = configuration;
        }
        /* 
        * Apply/Remove CouponCode to CartHeader in db  
        * if "" - empty string is passed - Remove coupon(set code to null in cartheader)
        * else coupon code is validated against code present in coupon db tables
        * If valid - enter into header
        * ifnot return invalid in response message
        */
        [HttpPost("ApplyCoupon")]
        public async Task<ResponseDto> ApplyCoupon([FromBody] CartDto cart)
        {
            try
            {
                var cartHeaderDb = _db.CartHeaders.First(h => h.UserId == cart.CartHeader.UserId);
                if (!string.IsNullOrEmpty(cart.CartHeader.CouponCode))
                {
                    var coupon = await _couponService.GetCoupon(cart.CartHeader.CouponCode);
                    if (coupon.CouponCode != null)
                    {
                        cartHeaderDb.CouponCode = cart.CartHeader.CouponCode;
                        _db.CartHeaders.Update(cartHeaderDb);
                        await _db.SaveChangesAsync();
                        _response.Result = true;
                    }
                    else
                    {
                        _response.IsSuccess = false;
                        _response.Result = false;
                        _response.Message = "Invalid Coupon";
                    }
                 }
                else
                {
                    cartHeaderDb.CouponCode = null;
                    _db.CartHeaders.Update(cartHeaderDb);
                    await _db.SaveChangesAsync();
                    _response.Result = true;
                    _response.Message = "Coupon Removed!";
                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                _response.Result = false;
            }
            return _response;
        }

        ///* 
        // * Remove CouponCode to CartHeader in db                
        // */
        //[HttpPost("RemoveCoupon")]
        //public async Task<ResponseDto> RemoveCoupon([FromBody] CartDto cart)
        //{
        //    try
        //    {
        //        var cartHeaderDb = _db.CartHeaders.First(h => h.UserId == cart.CartHeader.UserId);
        //        cartHeaderDb.CouponCode = "";
        //        _db.CartHeaders.Update(cartHeaderDb);
        //        await _db.SaveChangesAsync();
        //        _response.Result = true;

        //    }
        //    catch (Exception ex)
        //    {
        //        _response.IsSuccess = false;
        //        _response.Message = ex.Message;
        //        _response.Result = false;
        //    }
        //    return _response;
        //}

        // On clicking on Cart Button, contents to be displayed

        [HttpGet("GetCart/{userId}")]
        public async Task<ResponseDto> GetCart(string userId)
        {           
            try
            {
                CartDto cart = new()
                {
                    CartHeader = _mapper.Map<CartHeaderDto>(_db.CartHeaders.First(h => h.UserId == userId))
                };
                cart.CartDetails = _mapper.Map<IEnumerable<CartDetailDto>>(_db.CartDetails.Where(c =>
                                                c.CartHeaderId == cart.CartHeader.CartHeaderId).ToList());
                // product api call to get products
                var productDtos = await _productService.GetProducts();
                foreach (var item in cart.CartDetails)
                {
                    item.Product = productDtos.First( p => p.Id == item.ProductId);
                    cart.CartHeader.CartTotal += (item.Count * item.Product.Price);
                }

                //coupon api call to get coupondto of resp coupon
                if(!string.IsNullOrEmpty(cart.CartHeader.CouponCode))
                {
                     var coupon = await _couponService.GetCoupon(cart.CartHeader.CouponCode);
                    if (coupon != null && cart.CartHeader.CartTotal >= coupon.MinAmount)
                    {
                        cart.CartHeader.CartTotal -= coupon.DiscountAmount;
                        cart.CartHeader.Discount = coupon.DiscountAmount;
                    }
                }   

                _response.Result = cart;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                _response.Result = null;
            }
            return _response;

        }

        // Insert / Update item(s) into the cart of current signed-in user

        [HttpPost("CartUpsert")]
        public async Task<ResponseDto> CartUpsert(CartDto cartDto)
        {
            try
            {
                var cartHeaderFromDb = _db.CartHeaders.FirstOrDefault(u => u.UserId == cartDto.CartHeader.UserId);
                if (cartHeaderFromDb == null)
                {
                    // create cartheader and cartdetails to Db
                    CartHeader cartHeader = _mapper.Map<CartHeader>(cartDto.CartHeader);
                    _db.CartHeaders.Add(cartHeader);
                    await _db.SaveChangesAsync();
                    // cartdetail
                    cartDto.CartDetails.First().CartHeaderId = cartHeader.CartHeaderId;
                    _db.CartDetails.Add(_mapper.Map<CartDetail>(cartDto.CartDetails.First()));
                    await _db.SaveChangesAsync();
                }
                else
                { // if header is not null
                  // check if same Product is already present

                    var cardDetailFromDb = _db.CartDetails.AsNoTracking().FirstOrDefault(u => (u.ProductId == cartDto.CartDetails.First().ProductId && u.CartHeaderId == cartHeaderFromDb.CartHeaderId));

                    if (cardDetailFromDb == null)
                    {
                        // create cartdetails | link product with cartHeaderId
                        cartDto.CartDetails.First().CartHeaderId = cartHeaderFromDb.CartHeaderId;
                        _db.CartDetails.Add(_mapper.Map<CartDetail>(cartDto.CartDetails.First()));
                        await _db.SaveChangesAsync();

                    }
                    else
                    {
                        // update count of product in cartdetail
                        cartDto.CartDetails.First().Count += cardDetailFromDb.Count;
                        cartDto.CartDetails.First().CartHeaderId = cardDetailFromDb.CartHeaderId;
                        cartDto.CartDetails.First().CartDetailId = cardDetailFromDb.CartDetailId;
                        _db.CartDetails.Update(_mapper.Map<CartDetail>(cartDto.CartDetails.First()));
                        await _db.SaveChangesAsync();
                    }

                }
                _response.Result = cartDto;

            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        [HttpDelete("RemoveCart/{cartDetailId}")]
        public async Task<ResponseDto> RemoveCart(int cartDetailId)
        {
            try
            {
                CartDetail cartItem = _db.CartDetails.First(u => u.CartDetailId == cartDetailId);
                int totalCountofCartItem  = _db.CartDetails.Where(u => u.CartHeaderId == cartItem.CartHeaderId).Count();
                _db.CartDetails.Remove(cartItem);
                if(totalCountofCartItem == 1)
                {
                    var cartHeadertoRemove = _db.CartHeaders.First( h => h.CartHeaderId == cartItem.CartHeaderId);
                    _db.CartHeaders.Remove(cartHeadertoRemove);
                }
                await _db.SaveChangesAsync();
                _response.Result = true;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                _response.Result = false;
            }
            return _response;            
        }

        [HttpPost("EmailCartRequest")]
        public async Task<ResponseDto> EmailCartRequest([FromBody] CartDto cartDto)
        {
            try
            {
               await _messageBus.PublishMessage(cartDto, _configuration.GetValue<string>("TopicAndQueueNames:EmailShoppingCartQueue"));
                _response.Result = true;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                _response.Result = false;
            }
            return _response;
        }

    }
}
