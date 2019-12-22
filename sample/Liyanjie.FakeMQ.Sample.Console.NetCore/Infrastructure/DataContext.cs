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

            var fakeMQEventTypeBuilder = modelBuilder.Entity<FakeMQEvent>();
            fakeMQEventTypeBuilder.HasKey(_ => _.Id);
            fakeMQEventTypeBuilder.Property(_ => _.Type).IsRequired().HasMaxLength(50);
            fakeMQEventTypeBuilder.Property(_ => _.Message).IsRequired();
            fakeMQEventTypeBuilder.HasIndex(_ => _.Type);
            fakeMQEventTypeBuilder.HasIndex(_ => _.CreateTime);

            var fakeMQProcessTypeBuilder = modelBuilder.Entity<FakeMQProcess>();
            fakeMQProcessTypeBuilder.HasKey(_ => _.HandlerType);
            fakeMQProcessTypeBuilder.Property(_ => _.HandlerType).HasMaxLength(200);
            fakeMQProcessTypeBuilder.Property(_ => _.MessageType).IsRequired().HasMaxLength(200);

            var messageTypeBuilder = modelBuilder.Entity<Message>();
            messageTypeBuilder.HasKey(_ => _.Id);
            messageTypeBuilder.Property(_ => _.Content).HasMaxLength(500);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            optionsBuilder.UseSqlite(@"Data Source=.\Database.sqlite");
        }
    }
}
