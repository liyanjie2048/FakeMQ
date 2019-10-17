using System;

namespace Liyanjie.FakeMQ
{
    /// <summary>
    /// 
    /// </summary>
    public  class FakeMQProcess
    {
        /// <summary>
        /// 
        /// </summary>
        public string Subscription { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public long Timestamp { get; set; } = DateTimeOffset.Now.Ticks;
    }
}
