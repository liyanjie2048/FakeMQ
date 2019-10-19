using System;

namespace Liyanjie.FakeMQ
{
    /// <summary>
    /// 
    /// </summary>
    public class FakeMQDefaults
    {
        /// <summary>
        /// 
        /// </summary>
        public static Func<object, string> Serialize { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public static Func<string, Type, object> Deserialize { get; set; }
    }
}
