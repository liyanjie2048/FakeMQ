using Liyanjie.FakeMQ.Sample.Console.NetCore.Infrastructure.EntityConfigurations;
using Liyanjie.FakeMQ.Sample.Console.NetCore.Models;

using Microsoft.EntityFrameworkCore;

namespace Liyanjie.FakeMQ.Sample.Console.NetCore.Infrastructure
{
    public class DataContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public DbSet<FakeMQEvent> FakeMQEvents { get; set; }
        public DbSet<FakeMQProcess> FakeMQProcesses { get; set; }

        public DbSet<Message> Messages { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new FakeMQEventConfiguration());
            modelBuilder.ApplyConfiguration(new FakeMQProcessConfiguration());
            modelBuilder.ApplyConfiguration(new MessageConfiguration());
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            optionsBuilder.UseSqlite(@"Data Source=.\Database.sqlite");
        }
    }
}
