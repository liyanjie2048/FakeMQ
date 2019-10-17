using Liyanjie.FakeMQ;

namespace Liyanjie.FakeMQ.Sample.ConsoleApp.Models
{
    public class MessageEvent : IFakeMQEventMessage
    {
        public string Message { get; set; }
    }
}
