using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Mango.Fro.AuthAPI.Models;

namespace Mango.Fro.AuthAPI.Data
{                                                                                                                              

    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        /// <summary>
        /// Default setting required for EFCore
        /// </summary>
        /// <param name="options"></param>
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            
        }
         public DbSet<ApplicationUser> ApplicationUsers { get; set; } 
        protected override void OnModelCreating(ModelBuilder modelBuilder) {
        
            base.OnModelCreating(modelBuilder);          
        }
    }
}
