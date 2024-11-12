using Microsoft.EntityFrameworkCore;
using Mango.Services.OrderAPI.Models;

namespace Mango.Services.OrderAPI.Data
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

        public DbSet<OrderHeader> OrderHeaders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
    }
}


