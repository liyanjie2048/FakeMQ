using System.Data.Entity.ModelConfiguration;

namespace Liyanjie.FakeMQ.Sample.Console.Net.Infrastructure.EntityConfigurations
{
    public class FakeMQProcessConfiguration : EntityTypeConfiguration<FakeMQProcess>
    {
        public FakeMQProcessConfiguration()
        {
            this.HasKey(_ => _.Subscription);
        }
    }
}
