using Liyanjie.FakeMQ.Sample.AspNetCore.Domains;
using Liyanjie.FakeMQ.Sample.AspNetCore.Infrastructure.EntityConfigurations;

using Microsoft.EntityFrameworkCore;

namespace Liyanjie.FakeMQ.Sample.AspNetCore.Infrastructure
{
    public class DataContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<Message> Messages { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new MessageConfiguration());
        }
    }
}
