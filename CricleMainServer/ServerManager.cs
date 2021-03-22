using CricleMainServer.Network;
using System;
using System.Threading;

namespace CricleMainServer
{
    //该服务器socket连接使用异步连接方式，客户端解析数据使用线程池
    //
    /// <summary>
    /// 服务器管理类
    /// </summary>
    class ServerManager:IServerManager
    {
        private ListenServer listenServer;
        private ClientConnPool connPool;

        

        public void OpenListenServer()
        {
            throw new NotImplementedException();
        }

        public void OpenServer()
        {
            throw new NotImplementedException();
        }

        public void StopListenServer()
        {
            throw new NotImplementedException();
        }

        public void StopServer()
        {
            throw new NotImplementedException();
        }

        //初始化程序线程池
        public void InitThreadPool()
        {
            ThreadPool.SetMinThreads(2, 2);
            ThreadPool.SetMaxThreads(4, 4);
        }
    }
}
