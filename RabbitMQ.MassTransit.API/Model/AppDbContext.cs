using Microsoft.EntityFrameworkCore;

namespace RabbitMQ.MassTransit.API.Model
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<MessageBox> MessageBoxs { get; set; }
    }
}
