﻿namespace Mango.Services.OrderAPI.Models.Dto
{
    public class StripeRequestDto
    {
        public string? StripeSessionId { get; set; }
        public string? StripeSessionUrl { get; set; }
        public string CancelUrl { get; set; }
        public string ApprovedUrl { get; set; }
        public OrderHeaderDto OrderHeader { get; set; }
    }
}
