using Microsoft.EntityFrameworkCore;
using Entities;
using Entities.Mapping;

namespace Orders.WebAPI.ApplicationDbContext
{
    public class AppDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public AppDbContext(DbContextOptions options): base(options)
        {
            
        }

        public DbSet<Order> Orders { get; set; }

        public DbSet<OrderItem> OrderItems { get; set; }

        public DbSet<OrderItemReview> OrderItemReviews { get; set; }

        public DbSet<Country> Countries { get; set; }

        public DbSet<Customer> Customers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new OrderItemReviewMapping());

            modelBuilder.ApplyConfiguration(new CustomerMapping());

            modelBuilder.ApplyConfiguration(new OrderMapping());

            modelBuilder.ApplyConfiguration(new OrderItemMapping());

            base.OnModelCreating(modelBuilder);
        }

    }
}
