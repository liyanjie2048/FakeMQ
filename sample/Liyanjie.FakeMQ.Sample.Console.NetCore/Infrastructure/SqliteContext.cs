using Liyanjie.FakeMQ.Sample.Console.NetCore.Infrastructure.EntityConfigurations;
using Liyanjie.FakeMQ.Sample.Console.NetCore.Models;

using Microsoft.EntityFrameworkCore;

namespace Liyanjie.FakeMQ.Sample.Console.NetCore.Infrastructure
{
    public class SqliteContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public SqliteContext()
            : base(new DbContextOptionsBuilder<SqliteContext>().UseSqlite(@"Data Source=.\Database.sqlite").Options)
        { }

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
    }
}
