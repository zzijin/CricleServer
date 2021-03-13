using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CricleMainServer.Network.Configuration
{
    class ServerConfiguration
    {
        /// <summary>
        /// 服务器IP
        /// </summary>
        public static readonly IPAddress IP = IPAddress.Any;
        /// <summary>
        /// 服务器端口
        /// </summary>
        public static readonly int PORT = 12345;
        /// <summary>
        /// 最大连接客户端数
        /// </summary>
        public static readonly int CONN_SIZE = 100;
        /// <summary>
        /// 缓冲区大小
        /// </summary>
        public static readonly int BUFF_SIZE = 10 * 1024;
    }
}
