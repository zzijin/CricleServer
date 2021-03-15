using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CricleMainServer.Network;

namespace CricleMainServer
{
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
    }
}
