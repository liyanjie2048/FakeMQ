using System;

namespace Liyanjie.FakeMQ
{
    /// <summary>
    /// 
    /// </summary>
    public class FakeMQProcess
    {
        /// <summary>
        /// 
        /// </summary>
        public string HandlerType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTimeOffset LastHandleTime { get; set; } = DateTimeOffset.Now;
    }
}
