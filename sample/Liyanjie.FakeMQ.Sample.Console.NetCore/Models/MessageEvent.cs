using Liyanjie.FakeMQ;

namespace Liyanjie.FakeMQ.Sample.Console.NetCore.Models
{
    public class MessageEvent : IFakeMQEventMessage
    {
        public string Message { get; set; }
    }
}
