using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        /// <returns></returns>
        Task AddAsync(FakeMQEvent @event);

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
        Task<IEnumerable<FakeMQEvent>> GetAsync(string type, DateTimeOffset fromTime, DateTimeOffset toTime);

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
