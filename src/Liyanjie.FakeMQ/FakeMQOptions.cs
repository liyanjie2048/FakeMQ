using System;
#if NET45
using Newtonsoft.Json;
#else
using System.Text.Json;
#endif

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
#if NET45
            = obj => JsonConvert.SerializeObject(obj);
#else
            = obj => JsonSerializer.Serialize(obj);
#endif

        /// <summary>
        /// 反序列化
        /// </summary>
        public Func<string, Type, object> Deserialize { get; set; }
#if NET45
            = (str, type) => JsonConvert.DeserializeObject(str, type);
#else
            = (str, type) => JsonSerializer.Deserialize(str, type);
#endif
        /// <summary>
        /// 处理事件循环间隔
        /// </summary>
        public TimeSpan LoopTimeSpan { get; set; } = TimeSpan.FromMilliseconds(1000);
    }
}
