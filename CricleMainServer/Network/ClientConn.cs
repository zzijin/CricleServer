using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CricleMainServer.Network
{

    class ClientConn
    {
        private int poolIndex;

        public ClientConn(int index)
        {
            poolIndex = index;
        }
    }
}
