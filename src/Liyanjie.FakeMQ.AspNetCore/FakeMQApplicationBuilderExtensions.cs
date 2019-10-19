#if NET451 || NETSTANDARD1_5
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
        /// <param name="app"></param>
        public static void RunFakeMQ(this IApplicationBuilder app)
        {
            _ = new FakeMQBackgroundService(app.ApplicationServices.GetRequiredService<FakeMQEventBus>())
                .StartAsync(app.ApplicationServices.GetRequiredService<IApplicationLifetime>().ApplicationStopping);
        }
    }
}
#endif