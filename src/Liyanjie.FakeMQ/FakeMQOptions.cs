using System;

namespace Liyanjie.FakeMQ
{
    /// <summary>
    /// 
    /// </summary>
    public class FakeMQOptions
    {
        /// <summary>
        /// 
        /// </summary>
        public Func<object, string> Serialize { get; set; }
        /// <summary>
        /// 
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
        /// 
        /// </summary>
        public TimeSpan EventStoreCleaningLoopTimeSpan { get; set; } = TimeSpan.FromMinutes(5);
        /// <summary>
        /// 
        /// </summary>
        public TimeSpan EventHandlingLoopTimeSpan { get; set; } = TimeSpan.FromMilliseconds(500);
    }
}
