using Liyanjie.FakeMQ;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Liyanjie.FakeMQ.Sample.AspNetCore.Infrastructure.EntityConfigurations
{
    public class FakeMQProcessConfiguration : IEntityTypeConfiguration<FakeMQProcess>
    {
        public void Configure(EntityTypeBuilder<FakeMQProcess> builder)
        {
            builder.HasKey(_ => _.Subscription);
        }
    }
}
