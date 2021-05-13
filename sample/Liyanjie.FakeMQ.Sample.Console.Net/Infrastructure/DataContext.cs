using System.Data.Entity;

using Liyanjie.FakeMQ.Sample.Console.Net.Models;

namespace Liyanjie.FakeMQ.Sample.Console.Net.Infrastructure
{
    public class DataContext : Liyanjie.FakeMQ.FakeMQDbContext
    {
        public DataContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
            Database.SetInitializer(new CreateDatabaseIfNotExists<DataContext>());
        }

        public IDbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var messageTypeBuilder = modelBuilder.Entity<Message>();
            messageTypeBuilder.HasKey(_ => _.Id);
            messageTypeBuilder.Property(_ => _.Content).HasMaxLength(500);
        }
    }
}
