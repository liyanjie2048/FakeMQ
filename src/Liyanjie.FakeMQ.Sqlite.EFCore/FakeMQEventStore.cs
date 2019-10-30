﻿using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace Liyanjie.FakeMQ
{
    /// <summary>
    /// 
    /// </summary>
    public class FakeMQEventStore : IFakeMQEventStore, IDisposable
    {
        readonly FakeMQContext context;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public FakeMQEventStore(FakeMQContext context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            this.context?.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        public async Task<bool> AddAsync(FakeMQEvent @event)
        {
            context.FakeMQEvents.Add(@event);
            return await SaveAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public async Task<FakeMQEvent> GetAsync(string type, long timestamp)
        {
            return await context.FakeMQEvents.AsNoTracking()
                .Where(_ => _.Type == type && _.Timestamp > timestamp)
                .OrderBy(_ => _.Timestamp)
                .FirstOrDefaultAsync();
        }

        async Task<bool> SaveAsync()
        {
            try
            {
                await context.SaveChangesAsync();
                return true;
            }
            catch { }
            return false;
        }
    }
}