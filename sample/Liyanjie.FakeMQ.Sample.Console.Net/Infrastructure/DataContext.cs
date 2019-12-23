using System.Data.Entity;

using Liyanjie.FakeMQ.Sample.Console.Net.Models;

namespace Liyanjie.FakeMQ.Sample.Console.Net.Infrastructure
{
    public class DataContext : System.Data.Entity.DbContext
    {
        public DataContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
            Database.SetInitializer(new CreateDatabaseIfNotExists<DataContext>());
        }

        public IDbSet<FakeMQEvent> FakeMQEvents { get; set; }
        public IDbSet<FakeMQProcess> FakeMQProcesses { get; set; }

        public IDbSet<Message> Messages { get; set; }

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

            var messageTypeBuilder = modelBuilder.Entity<Message>();
            messageTypeBuilder.HasKey(_ => _.Id);
            messageTypeBuilder.Property(_ => _.Content).HasMaxLength(500);
        }
    }
}
