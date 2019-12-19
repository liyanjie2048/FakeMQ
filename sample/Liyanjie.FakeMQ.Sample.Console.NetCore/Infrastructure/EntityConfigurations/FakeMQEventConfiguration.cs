using Liyanjie.FakeMQ;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Liyanjie.FakeMQ.Sample.Console.NetCore.Infrastructure.EntityConfigurations
{
    public class FakeMQEventConfiguration : IEntityTypeConfiguration<FakeMQEvent>
    {
        public void Configure(EntityTypeBuilder<FakeMQEvent> builder)
        {
            builder.Property(_ => _.Type).HasMaxLength(50);

            builder.HasKey(_ => _.Id);

            builder.HasIndex(_ => _.Type);
            builder.HasIndex(_ => _.CreateTime);
        }
    }
}
