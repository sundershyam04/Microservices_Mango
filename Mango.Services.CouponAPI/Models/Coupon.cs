using System.ComponentModel.DataAnnotations;

namespace Mango.Fro.CouponAPI.Models
{
    /*<summary>
Coupon to be applied on Products
    </summary>*/
    public class Coupon
    {
        [Key]
        public int CouponId { get; set; }
        [Required]
        public string CouponCode { get; set; } = string.Empty;
        [Required]
        public double DiscountAmount {  get; set; }
        public int MinAmount { get; set; }

    }
}
