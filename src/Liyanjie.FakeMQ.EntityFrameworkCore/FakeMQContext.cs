using Microsoft.EntityFrameworkCore;

namespace Liyanjie.FakeMQ
{
    /// <summary>
    /// 
    /// </summary>
    public class FakeMQContext : DbContext
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        public FakeMQContext(DbContextOptions<FakeMQContext> options)
            : base(options) { }

        /// <summary>
        /// 
        /// </summary>
        public DbSet<FakeMQEvent> FakeMQEvents { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DbSet<FakeMQProcess> FakeMQProcesses { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var fakeMQEventTypeBuilder = modelBuilder.Entity<FakeMQEvent>();
            fakeMQEventTypeBuilder.HasKey(_ => _.Id);
            fakeMQEventTypeBuilder.Property(_ => _.Type).HasMaxLength(50);
            fakeMQEventTypeBuilder.HasIndex(_ => _.Type);
            fakeMQEventTypeBuilder.HasIndex(_ => _.Timestamp);

            var fakeMQProcessTypeBuilder = modelBuilder.Entity<FakeMQProcess>();
            fakeMQProcessTypeBuilder.HasKey(_ => _.Subscription);
            fakeMQProcessTypeBuilder.Property(_ => _.Subscription).HasMaxLength(200);
        }
    }
}
