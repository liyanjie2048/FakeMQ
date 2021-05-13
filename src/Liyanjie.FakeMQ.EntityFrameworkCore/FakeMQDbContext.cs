using System;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Liyanjie.FakeMQ
{
    /// <summary>
    /// 
    /// </summary>
    public class FakeMQDbContext : DbContext
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        public FakeMQDbContext(DbContextOptions<FakeMQDbContext> options)
            : base(options) { }

        /// <summary>
        /// 
        /// </summary>
        public DbSet<FakeMQEvent> FakeMQEvents { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DbSet<FakeMQProcess> FakeMQProcesses { get; set; }

        /// <inheritdoc/>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var fakeMQEventTypeBuilder = modelBuilder.Entity<FakeMQEvent>();
            fakeMQEventTypeBuilder.HasKey(_ => _.CreateTime);
            fakeMQEventTypeBuilder.Property(_ => _.Type).IsRequired().HasMaxLength(50);
            fakeMQEventTypeBuilder.Property(_ => _.Message).IsRequired();
            fakeMQEventTypeBuilder.HasIndex(_ => _.Type);

            var fakeMQProcessTypeBuilder = modelBuilder.Entity<FakeMQProcess>();
            fakeMQProcessTypeBuilder.HasKey(_ => _.HandlerType);
            fakeMQProcessTypeBuilder.Property(_ => _.HandlerType).HasMaxLength(200);
        }

        internal static FakeMQDbContext GetContext(IServiceProvider serviceProvider)
        {
            return serviceProvider.CreateScope().ServiceProvider.GetRequiredService<FakeMQDbContext>();
        }
    }
}
