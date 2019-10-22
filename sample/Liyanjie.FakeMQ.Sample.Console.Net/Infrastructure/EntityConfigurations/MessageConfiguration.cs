using System.Data.Entity.ModelConfiguration;

using Liyanjie.FakeMQ.Sample.Console.Net.Models;

namespace Liyanjie.FakeMQ.Sample.Console.Net.Infrastructure.EntityConfigurations
{
    public class MessageConfiguration : EntityTypeConfiguration<Message>
    {
        public MessageConfiguration()
        {
            this.HasKey(_ => _.Id);
        }
    }
}
