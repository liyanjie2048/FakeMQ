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
        public DateTimeOffset CreateTime { get; set; } = DateTimeOffset.Now;

        /// <summary>
        /// 
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Message { get; set; }
    }
}
