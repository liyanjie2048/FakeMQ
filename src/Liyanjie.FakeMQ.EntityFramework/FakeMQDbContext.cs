using System.Data.Entity;

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
        /// <param name="nameOrConnectionString"></param>
        public FakeMQDbContext(string nameOrConnectionString)
            : base(nameOrConnectionString) { }

        /// <summary>
        /// 
        /// </summary>
        public IDbSet<FakeMQEvent> FakeMQEvents { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IDbSet<FakeMQProcess> FakeMQProcesses { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
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
    }
}
