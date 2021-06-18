using System;

#if NETSTANDARD2_0||NETSTANDARD2_1
using Microsoft.Extensions.Logging;
#endif

namespace Liyanjie.FakeMQ
{
    /// <summary>
    /// 
    /// </summary>
    public class FakeMQLogger
    {
#if NETFRAMEWORK
        readonly NLog.ILogger logger;
        /// <summary>
        /// 
        /// </summary>
        public FakeMQLogger()
        {
            this.logger = NLog.LogManager.GetLogger(nameof(FakeMQ));
        }
#endif
#if NETSTANDARD
        readonly ILogger<FakeMQ> logger;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        public FakeMQLogger(ILogger<FakeMQ> logger)
        {
            this.logger = logger;
        }
#endif

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void LogTrace(string message)
        {
#if NETFRAMEWORK
            logger?.Trace(message);
#endif
#if NETSTANDARD
            logger?.LogTrace(message);
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void LogDebug(string message)
        {
#if NETFRAMEWORK
            logger?.Debug(message);
#endif
#if NETSTANDARD
            logger?.LogDebug(message);
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void LogInformation(string message)
        {
#if NETFRAMEWORK
            logger?.Info(message);
#endif
#if NETSTANDARD
            logger?.LogInformation(message);
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void LogWarning(string message)
        {
#if NETFRAMEWORK
            logger?.Warn(message);
#endif
#if NETSTANDARD
            logger?.LogWarning(message);
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void LogError(string message)
        {
#if NETFRAMEWORK
            logger?.Error(message);
#endif
#if NETSTANDARD
            logger?.LogError(message);
#endif
        }

    }
}
