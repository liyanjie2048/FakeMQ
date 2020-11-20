using System;
using System.Collections.Generic;

namespace Liyanjie.FakeMQ
{
    /// <summary>
    /// 
    /// </summary>
    public interface IFakeMQEventStore
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="event"></param>
        void Add(FakeMQEvent @event);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="fromTime"></param>
        /// <param name="toTime"></param>
        /// <returns></returns>
        IEnumerable<FakeMQEvent> Get(string type, DateTimeOffset fromTime, DateTimeOffset toTime);
    }
}
