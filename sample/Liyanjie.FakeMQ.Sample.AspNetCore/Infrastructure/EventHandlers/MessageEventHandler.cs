using System;
using System.Threading.Tasks;

using Liyanjie.FakeMQ;

using Liyanjie.FakeMQ.Sample.AspNetCore.Domains;

namespace Liyanjie.FakeMQ.Sample.AspNetCore.Infrastructure.EventHandlers
{
    public class MessageEventHandler : IFakeMQEventHandler<MessageEvent>, IDisposable
    {
        readonly SqliteContext context;
        public MessageEventHandler(SqliteContext context)
        {
            this.context = context;
        }

        public void Dispose()
        {
            context?.Dispose();
        }

        public async Task<bool> HandleAsync(MessageEvent @event)
        {
            context.Messages.Add(new Message
            {
                Content = @event.Message,
            });
            await context.SaveChangesAsync();

            return true;
        }
    }
}
