using System.Threading.Tasks;

using Liyanjie.FakeMQ.Sample.Console.Net.Models;

namespace Liyanjie.FakeMQ.Sample.Console.Net.Infrastructure.EventHandlers
{
    public class MessageEventHandler : IFakeMQEventHandler<MessageEvent>
    {
        public async Task<bool> HandleAsync(MessageEvent @event)
        {
            try
            {
                using var context = new DataContext(Program.ConnectionString);
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
