using System;

namespace Liyanjie.FakeMQ.Sample.AspNetCore_2_1.Domains
{
    public class Message
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Content { get; set; }
    }
}
