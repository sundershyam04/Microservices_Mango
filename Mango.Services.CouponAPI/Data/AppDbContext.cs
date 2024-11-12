using Microsoft.EntityFrameworkCore;
using Mango.Fro.CouponAPI.Models;

namespace Mango.Fro.CouponAPI.Data
{                                                                                                                          
    public class AppDbContext : DbContext
    {
        /// <summary>
        /// Default setting required for EFCore
        /// </summary>
        /// <param name="options"></param>
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            
        }
        public DbSet<Coupon> Coupons { get; set; }
    

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
        
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Coupon>().HasData(new Coupon
            {
                CouponId = 1,
                CouponCode = "50OFF",
                DiscountAmount = 50,
                MinAmount = 500
            });

            modelBuilder.Entity<Coupon>().HasData(new Coupon
            {
                CouponId = 2,
                CouponCode = "20OFF",
                DiscountAmount =20,
                MinAmount=250
            });

        }
    }
}
