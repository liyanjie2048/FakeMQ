using Liyanjie.FakeMQ;

using Microsoft.EntityFrameworkCore;

using Liyanjie.FakeMQ.Sample.AspNetCore.Domains;
using Liyanjie.FakeMQ.Sample.AspNetCore.Infrastructure.EntityConfigurations;

namespace Liyanjie.FakeMQ.Sample.AspNetCore.Infrastructure
{
    public class SqliteContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public SqliteContext(DbContextOptions<SqliteContext> options) : base(options) { }

        public DbSet<FakeMQEvent> FakeMQEvents { get; set; }
        public DbSet<FakeMQProcess> FakeMQProcesses { get; set; }

        public DbSet<Message> Messages { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new EventConfiguration());
            modelBuilder.ApplyConfiguration(new ProcessConfiguration());
            modelBuilder.ApplyConfiguration(new MessageConfiguration());
        }
    }
}
