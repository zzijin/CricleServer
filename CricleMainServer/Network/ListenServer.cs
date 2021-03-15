using CricleMainServer.Network.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CricleMainServer.Network
{
    /// <summary>
    /// 服务器接入类
    /// </summary>
     class ListenServer
    {
        private Socket listenServer;

        /// <summary>
        /// 开始监听端口
        /// </summary>
        public void OpenListenServer()
        {
            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, ServerConfiguration.PORT);
            listenServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listenServer.Bind(iPEndPoint);

            //排队等待连接的最大数量，注意这个数量不包含已经连接的数量
            listenServer.Listen(10);

            listenServer.BeginAccept(AcceptCb, null);
        }

        /// <summary>
        /// 监听客户端回调函数
        /// </summary>
        /// <param name="ar">异步操作状态</param>
        private void AcceptCb(IAsyncResult ar)
        {

        }

        /// <summary>
        /// 停止监听端口
        /// </summary>
        public void StopListenServer()
        {

        }
    }
}
