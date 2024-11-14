using AutoMapper;
using Mango.Messaging;
using Mango.Services.OrderAPI.Data;
using Mango.Services.OrderAPI.Models;
using Mango.Services.OrderAPI.Models.Dto;
using Mango.Services.OrderAPI.Service.IService;
using Mango.Services.OrderAPI.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;

namespace Mango.Services.OrderAPI.Controllers
{
    [Route("api/order")]
    [ApiController]
    public class OrderAPIController : ControllerBase
    {
        private ResponseDto _response;
        private readonly IProductService _productService;
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        private readonly IMessageBus _messageBus;
        private readonly IConfiguration _configuration;


        public OrderAPIController(IProductService productService, AppDbContext db, IMapper mapper, IMessageBus messageBus, IConfiguration configuration )
        {
            this._response = new();
            _productService = productService;
            _db = db;
            _mapper = mapper;
            _messageBus = messageBus;
            _configuration = configuration;
        }

        [Authorize]
        [HttpPost("CreateOrder")]
        public async Task<ResponseDto> CreateOrder([FromBody] CartDto cartDto)
        {
            try
            {
               OrderHeaderDto orderHeaderDto =  _mapper.Map<OrderHeaderDto>(cartDto.CartHeader);
               orderHeaderDto.OrderDetails = _mapper.Map<IEnumerable<OrderDetailDto>>(cartDto.CartDetails);
               orderHeaderDto.OrderTime = DateTime.Now;
               orderHeaderDto.Status = SD.Status_Pending;

               OrderHeader orderCreated = _db.OrderHeaders.Add(_mapper.Map<OrderHeader>(orderHeaderDto)).Entity;
               await _db.SaveChangesAsync();

                orderHeaderDto.OrderHeaderId = orderCreated.OrderHeaderId;
                _response.Result = orderHeaderDto;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        [Authorize]
        [HttpPost("CreateStripeSession")]
        /* Creates a Stripe session integrating with stripe library
         * Input parameter: stripeRequestDto
         */
        public async Task<ResponseDto> CreateStripeSession([FromBody] StripeRequestDto stripeRequestDto)
        {

            try
            {
                var options = new SessionCreateOptions
                {
                    SuccessUrl = stripeRequestDto.ApprovedUrl,
                    CancelUrl = stripeRequestDto.CancelUrl,
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",                   
                };


                foreach (var item in stripeRequestDto.OrderHeader.OrderDetails)
                {
                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Price * 100), // usd --> cents  $2 --> 200cents
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.ProductName
                            }
                        },
                        Quantity = item.Count
                    };
                    options.LineItems.Add(sessionLineItem);
                }

              
                if (stripeRequestDto.OrderHeader.Discount > 0) 
                {
                    List<SessionDiscountOptions> discountObj = new()
                        {
                            new()
                            {
                                Coupon = stripeRequestDto.OrderHeader.CouponCode
                            }
                        };
                    options.Discounts = discountObj;
                }
                
                var service = new SessionService(); //create stripe session
                Session session = service.Create(options);
                stripeRequestDto.StripeSessionUrl = session.Url;

                // retrieve ordeHeader from Db and store sessionId
                OrderHeader orderHeader =  await _db.OrderHeaders.FirstAsync(u => u.OrderHeaderId == stripeRequestDto.OrderHeader.OrderHeaderId);
                orderHeader.StripeSessionId = session.Id;
                await _db.SaveChangesAsync();
                _response.Result = stripeRequestDto;

            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;

            }
            return _response;
        }

        [Authorize]
        [HttpPost("ValidateStripeSession")]
        /* Creates a Stripe session integrating with stripe library
         * Input parameter: stripeRequestDto
         */
        public async Task<ResponseDto> ValidateStripeSession([FromBody] int orderHeaderId)
        {

            try
            {
               OrderHeader orderHeader = _db.OrderHeaders.First(h => h.OrderHeaderId == orderHeaderId);

                var service = new SessionService(); // stripe session
                Session session = service.Get(orderHeader.StripeSessionId);

                PaymentIntentService paymentIntentService = new ();
                PaymentIntent paymentIntent = paymentIntentService.Get(session.PaymentIntentId);

                if( paymentIntent.Status == "succeeded")
                {
                    orderHeader.PaymentIntentId = paymentIntent.Id;
                    orderHeader.Status = SD.Status_Approved;
                    await _db.SaveChangesAsync();
                    // post order details to servicebus topic
                    RewardsDto rewardsDto = new()
                    {
                        UserId = orderHeader.UserId,
                        OrderId = orderHeader.OrderHeaderId,
                        RewardsActivity = Convert.ToInt32(orderHeader.OrderTotal)
                    };
                    await _messageBus.PublishMessage(rewardsDto, _configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedTopic"));
                    _response.Result = _mapper.Map<OrderHeaderDto>(orderHeader);
                }               
               
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
