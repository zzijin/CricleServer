using CricleMainServer.Network.Configuration;
using System;
using System.Net;
using System.Net.Sockets;

namespace CricleMainServer.Network
{
    /// <summary>
    /// 服务器接入类
    /// </summary>
    class ListenServer
    {
        private Socket listenServer;
        private DTryNewConn dTryNewConn;

        public ListenServer(DTryNewConn dTryNewConn)
        {
            this.dTryNewConn = dTryNewConn;
        }

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
            Console.WriteLine("[" + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds() +
                    "] [Class:ListenServer] -开始监听端口 -IP:"+ IPAddress.Any + " -Port:" + ServerConfiguration.PORT);
        }

        /// <summary>
        /// 监听客户端回调函数
        /// </summary>
        /// <param name="ar">异步操作状态</param>
        private void AcceptCb(IAsyncResult ar)
        {
            Socket newSocket = listenServer.EndAccept(ar);
            if (dTryNewConn(newSocket))
            {
                //建立连接成功
                Console.WriteLine("[" + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds() +
                    "] [Class:ListenServer] -链接池成功建立一个新连接,地址:" + newSocket.RemoteEndPoint.ToString());
            }
            else
            {
                //建立连接失败
                Console.WriteLine("[" + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds() +
                   "] [Class:ListenServer] -链接池建立新连接失败,地址:" + newSocket.RemoteEndPoint.ToString() + "无空连接");
            }
            listenServer.BeginAccept(AcceptCb, null);
            Console.WriteLine("[" + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds() +
                   "] [Class:ListenServer] -开始监听端口 -IP:" + IPAddress.Any + " -Port:" + ServerConfiguration.PORT);
        }

        /// <summary>
        /// 停止监听端口
        /// </summary>
        public void StopListenServer()
        {

        }
    }
}
