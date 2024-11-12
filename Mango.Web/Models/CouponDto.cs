namespace Mango.Web.Models;

/// <summary>
/// DTO to map coupon class - object made to transfer necessary data
/// </summary>
public class CouponDto
{  
    public int CouponId { get; set; }
    public required string CouponCode { get; set; }
    public double DiscountAmount { get; set; }
    public int MinAmount { get; set; }
}
