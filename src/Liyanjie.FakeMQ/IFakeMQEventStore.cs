using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Liyanjie.FakeMQ
{
    /// <summary>
    /// 
    /// </summary>
    public interface IFakeMQEventStore : IDisposable
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
        /// <param name="startTimestamp"></param>
        /// <param name="endTimestamp"></param>
        /// <returns></returns>
        Task<IEnumerable<FakeMQEvent>> GetAsync(string type, long startTimestamp,long endTimestamp);
    }
}
