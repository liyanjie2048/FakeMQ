using System;
using System.Threading.Tasks;

using Liyanjie.FakeMQ.Sample.AspNetCore_3_0.Domains;

namespace Liyanjie.FakeMQ.Sample.AspNetCore_3_0.Infrastructure.EventHandlers
{
    public class MessageEventHandler : IFakeMQEventHandler<MessageEvent>, IDisposable
    {
        readonly DataContext context;
        public MessageEventHandler(DataContext context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public void Dispose()
        {
            context?.Dispose();
        }

        public async Task<bool> HandleAsync(MessageEvent @event)
        {
            try
            {
                context.Messages.Add(new Message
                {
                    Content = @event.Message,
                });
                await context.SaveChangesAsync();
            }
            catch { }

            return true;
        }
    }
}
