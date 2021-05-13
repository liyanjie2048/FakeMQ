using System;

namespace Liyanjie.FakeMQ.Sample.AspNetCore.Domains
{
    public class Message
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Content { get; set; }
    }
}
