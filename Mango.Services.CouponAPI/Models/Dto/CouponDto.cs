namespace Mango.Fro.CouponAPI.Models.Dto
{
    /// <summary>
    /// DTO to map coupon class - object made to transfer necessary data
    /// </summary>
    public class CouponDto
    {  
        public int CouponId { get; set; }
        public string CouponCode { get; set; }
        public double DiscountAmount { get; set; }
        public int MinAmount { get; set; }
    }
}
