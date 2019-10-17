using Liyanjie.FakeMQ;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Liyanjie.FakeMQ.Sample.ConsoleApp.Infrastructure.EntityConfigurations
{
    public class ProcessConfiguration : IEntityTypeConfiguration<FakeMQProcess>
    {
        public void Configure(EntityTypeBuilder<FakeMQProcess> builder)
        {
            builder.HasKey(_ => _.Subscription);
        }
    }
}
