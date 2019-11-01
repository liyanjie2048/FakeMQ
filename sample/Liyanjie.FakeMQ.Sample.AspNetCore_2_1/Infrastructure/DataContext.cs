using Liyanjie.FakeMQ.Sample.AspNetCore_2_1.Domains;
using Liyanjie.FakeMQ.Sample.AspNetCore_2_1.Infrastructure.EntityConfigurations;

using Microsoft.EntityFrameworkCore;

namespace Liyanjie.FakeMQ.Sample.AspNetCore_2_1.Infrastructure
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
