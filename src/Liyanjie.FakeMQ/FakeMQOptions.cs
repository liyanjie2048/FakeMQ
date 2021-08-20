using System;
using System.Text.Json;

namespace Liyanjie.FakeMQ
{
    /// <summary>
    /// 
    /// </summary>
    public class FakeMQOptions
    {
        /// <summary>
        /// 处理事件循环间隔
        /// </summary>
        public TimeSpan LoopTimeSpan { get; set; } = TimeSpan.FromMilliseconds(1000);
    }
}
