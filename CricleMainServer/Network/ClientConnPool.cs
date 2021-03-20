using CricleMainServer.Network.Configuration;
using System.Net.Sockets;

namespace CricleMainServer.Network
{
    delegate Socket DFindSocketByUIDFromPool(int clientUID);
    delegate bool DTryNewConn(Socket socket);
    /// <summary>
    /// 客户端连接池
    /// </summary>
    class ClientConnPool
    {
        private ClientConn[] clientPool;

        public ClientConnPool()
        {
            InitPool();
        }

        public void InitPool()
        {
            DFindSocketByUIDFromPool dFindSocketByUID = new DFindSocketByUIDFromPool(FindSocketByUID);
            DTryNewConn dTryNewConn = new DTryNewConn(TryNewConn);

            //初始化客户链接池
            clientPool = new ClientConn[ServerConfiguration.CONN_SIZE];
            for (int i = 0; i < ServerConfiguration.CONN_SIZE; i++)
            {
                clientPool[i] = new ClientConn(i,dFindSocketByUID);
            }
        }

        public bool TryNewConn(Socket newSocket)
        {
            int freeConnIndex = FindFreeConn();
            if (freeConnIndex == -1)
            {
                return false;
            }
            clientPool[freeConnIndex].OpenClientConn(newSocket);

            return true;
        }

        /// <summary>
        /// 寻找链接池中空闲的链接
        /// </summary>
        /// <returns></returns>
        public int FindFreeConn()
        {
            for(int i = 0; i < ServerConfiguration.CONN_SIZE; i++)
            {
                if (!clientPool[i].ClientState)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// 寻找指定UID的链接位置
        /// </summary>
        /// <param name="clientUID"></param>
        /// <returns></returns>
        public Socket FindSocketByUID(int clientUID)
        {
            if (clientPool != null)
            {
                for (int i = 0; i < ServerConfiguration.CONN_SIZE; i++)
                {
                    if (clientPool[i].ClientUID == clientUID)
                    {
                        return clientPool[i].ClientSocket;
                    }
                }
            }
            return null;
        }
    }
}
