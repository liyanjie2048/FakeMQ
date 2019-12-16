using System;

namespace Liyanjie.FakeMQ
{
    /// <summary>
    /// 
    /// </summary>
    public class FakeMQOptions
    {
        /// <summary>
        /// 序列化
        /// </summary>
        public Func<object, string> Serialize { get; set; }

        /// <summary>
        /// 反序列化
        /// </summary>
        public Func<string, Type, object> Deserialize { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Func<IServiceProvider, IFakeMQEventStore> GetEventStore { get; set; }
            = serviceProvider => serviceProvider.GetService(typeof(IFakeMQEventStore)) as IFakeMQEventStore;

        /// <summary>
        /// 
        /// </summary>
        public Func<IServiceProvider, IFakeMQProcessStore> GetProcessStore { get; set; }
            = serviceProvider => serviceProvider.GetService(typeof(IFakeMQProcessStore)) as IFakeMQProcessStore;

        /// <summary>
        /// 处理事件循环间隔
        /// </summary>
        public TimeSpan LoopTimeSpan { get; set; } = TimeSpan.FromMilliseconds(1000);
    }
}
