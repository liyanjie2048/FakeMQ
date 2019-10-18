#if !NETSTANDARD2_0
using Liyanjie.FakeMQ;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// 
    /// </summary>
    public static class FakeMQApplicationBuilderExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEventStore"></typeparam>
        /// <typeparam name="TProcessStore"></typeparam>
        /// <param name="services"></param>
        /// <param name="jsonSerialize"></param>
        /// <param name="jsonDeserialize"></param>
        /// <returns></returns>
        public static void RunFakeMQ(this IApplicationBuilder app, IApplicationLifetime applicationLifetime)
        {
            _ = new FakeMQBackgroundService(app.ApplicationServices.GetService<FakeMQEventBus>())
                .StartAsync(applicationLifetime.ApplicationStopping);
        }
    }
}
#endif