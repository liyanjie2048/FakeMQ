using System;

namespace Liyanjie.FakeMQ
{
    /// <summary>
    /// 
    /// </summary>
    public class FakeMQEvent
    {
        /// <summary>
        /// 
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public long Timestamp { get; set; } = DateTimeOffset.Now.Ticks;

        internal object GetMsgObject(Type messageType)
            => FakeMQ.Deserialize(Message, messageType);

        internal static string GetMsgString<TEventMessage>(TEventMessage message)
            where TEventMessage : IFakeMQEventMessage
            => FakeMQ.Serialize(message);
    }
}
