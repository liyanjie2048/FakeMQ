using System;
using System.Threading.Tasks;

using Liyanjie.FakeMQ;

using Liyanjie.FakeMQ.Sample.Console.Net.Models;

namespace Liyanjie.FakeMQ.Sample.Console.Net.Infrastructure.EventHandlers
{
    public class MessageEventHandler : IFakeMQEventHandler<MessageEvent>, IDisposable
    {
        readonly SqlCeContext context;
        public MessageEventHandler(SqlCeContext context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
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
