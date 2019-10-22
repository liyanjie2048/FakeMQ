using System.Data.Entity.ModelConfiguration;

namespace Liyanjie.FakeMQ.Sample.Console.Net.Infrastructure.EntityConfigurations
{
    public class FakeMQEventConfiguration : EntityTypeConfiguration<FakeMQEvent>
    {
        public FakeMQEventConfiguration()
        {
            this.HasKey(_ => _.Id);
        }
    }
}
