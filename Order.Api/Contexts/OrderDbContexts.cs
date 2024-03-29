using Microsoft.EntityFrameworkCore;

namespace Order.Api.Contexts
{
    public class OrderDbContexts : DbContext
    {
        public OrderDbContexts(DbContextOptions dbContextOptions) : base(dbContextOptions)
        {
        }

        public DbSet<Models.Order> Orders { get; set; }
        public DbSet<Models.OrderItem> OrderItems { get; set; }
    }
}
