using Liyanjie.FakeMQ;

namespace Liyanjie.FakeMQ.Sample.AspNetCore.Domains
{
    public class MessageEvent : IFakeMQEventMessage
    {
        public string Message { get; set; }
    }
}
