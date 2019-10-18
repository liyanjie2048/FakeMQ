using Liyanjie.FakeMQ;

namespace Liyanjie.FakeMQ.Sample.Console.Net.Models
{
    public class MessageEvent : IFakeMQEventMessage
    {
        public string Message { get; set; }
    }
}
