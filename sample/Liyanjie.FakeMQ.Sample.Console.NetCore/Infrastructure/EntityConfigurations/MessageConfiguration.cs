using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Liyanjie.FakeMQ.Sample.Console.NetCore.Models;

namespace Liyanjie.FakeMQ.Sample.Console.NetCore.Infrastructure.EntityConfigurations
{
    public class MessageConfiguration : IEntityTypeConfiguration<Message>
    {
        public void Configure(EntityTypeBuilder<Message> builder)
        {
            builder.Property(_ => _.Content).HasMaxLength(500);

            builder.HasKey(_ => _.Id);
        }
    }
}
