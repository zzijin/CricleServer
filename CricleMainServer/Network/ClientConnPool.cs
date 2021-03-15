using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CricleMainServer.Network
{
    /// <summary>
    /// 客户端连接池
    /// </summary>
    class ClientConnPool
    {
        private ClientConn[] clientPool;

        public void InitPool()
        {
            clientPool = new ClientConn[CricleMainServer.Network.Configuration.ServerConfiguration.CONN_SIZE];

            for (int i = 0; i < CricleMainServer.Network.Configuration.ServerConfiguration.CONN_SIZE; i++)
            {
                clientPool[i] = new ClientConn(i);
            }
        }
    }
}
