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
#if NET45
        readonly NLog.ILogger logger;
        /// <summary>
        /// 
        /// </summary>
        public FakeMQLogger()
        {
            this.logger = NLog.LogManager.GetLogger(nameof(FakeMQ));
        }
#endif
#if NETSTANDARD2_0||NETSTANDARD2_1
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
#if NET45
            logger?.Trace(message);
#endif
#if NETSTANDARD2_0||NETSTANDARD2_1
            logger?.LogTrace(message);
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void LogDebug(string message)
        {
#if NET45
            logger?.Debug(message);
#endif
#if NETSTANDARD2_0||NETSTANDARD2_1
            logger?.LogDebug(message);
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void LogInformation(string message)
        {
#if NET45
            logger?.Info(message);
#endif
#if NETSTANDARD2_0||NETSTANDARD2_1
            logger?.LogInformation(message);
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void LogWarning(string message)
        {
#if NET45
            logger?.Warn(message);
#endif
#if NETSTANDARD2_0||NETSTANDARD2_1
            logger?.LogWarning(message);
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="message"></param>
        public void LogError(Exception exception, string message)
        {
#if NET45
            logger?.Error(exception, message);
#endif
#if NETSTANDARD2_0||NETSTANDARD2_1
            logger?.LogError(exception, message);
#endif
        }

    }
}
