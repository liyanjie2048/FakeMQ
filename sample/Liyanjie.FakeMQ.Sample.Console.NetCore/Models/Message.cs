using System;

namespace Liyanjie.FakeMQ.Sample.Console.NetCore.Models
{
    public class Message
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Content { get; set; }
    }
}
