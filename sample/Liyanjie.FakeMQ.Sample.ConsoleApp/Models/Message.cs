using System;

namespace Liyanjie.FakeMQ.Sample.ConsoleApp.Models
{
    public class Message
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Content { get; set; }
    }
}
