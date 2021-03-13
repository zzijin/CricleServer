using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CricleMainServer.Tools
{
    class ConvertTypeTool
    {
        /// <summary>
        /// 把int32类型的数据转存到4个字节的byte数组中
        /// </summary>
        /// <param name="m">int32类型的数据</param>
        /// <returns></returns>
        static public byte[] Int32ToByteArray(Int32 m)
        {
            byte[] arry = new byte[4];

            if (arry == null) return null;
            if (arry.Length < 4) return null;

            arry[0] = (byte)(m & 0xFF);
            arry[1] = (byte)((m & 0xFF00) >> 8);
            arry[2] = (byte)((m & 0xFF0000) >> 16);
            arry[3] = (byte)((m >> 24) & 0xFF);

            return arry;
        }

        /// <summary>
        /// 把4个字节的byte数组转存到int32类型的数据中
        /// </summary>
        /// <param name="arry">4个字节大小的byte数组</param>
        /// <returns></returns>
        static public Int32 ByteArrayToInt32(byte[] ar)
        {
            byte[] arry = new byte[ar.Length];
            arry = ar;

            Int32 m;

            if (arry == null)
                return -1;
            if (arry.Length != 4)
                return -1;

            m = BitConverter.ToInt32(arry, 0);
            return m;
        }

        /// <summary>
        /// 工具函数，用于合并数组
        /// </summary>
        /// <param name="a">第一个合并数组</param>
        /// <param name="b">第二个合并数组</param>
        /// <returns>合并完成的数组</returns>
        static public byte[] PooledByteArray(byte[] a, byte[] b)
        {
            byte[] c;
            //使用Buffer.BlockCopy合成数组优化性能
            c = new byte[a.Length + b.Length];
            System.Buffer.BlockCopy(a, 0, c, 0, a.Length);
            System.Buffer.BlockCopy(b, 0, c, a.Length, b.Length);
            return c;
        }

        /// <summary>
        /// 工具函数，用于合并数组(注意:本函数专用于封包，将默认开头结尾添加标识符0x99)
        /// </summary>
        /// <param name="a">第一个合并数组</param>
        /// <param name="b">第二个合并数组</param>
        /// <returns>合并完成的数组</returns>
        static public byte[] PooledByteArrayForPackMessage(byte[] a, byte[] b)
        {
            byte[] c = new byte[a.Length + b.Length + 2];

            //使用Buffer.BlockCopy合成数组优化性能

            System.Buffer.BlockCopy(a, 0, c, 1, a.Length);
            System.Buffer.BlockCopy(b, 0, c, a.Length + 1, b.Length);
            c[0] = SocketConfiguration.DATA_START_TAG;
            c[c.Length - 1] = SocketConfiguration.DATA_END_TAG;

            return c;
        }

        /// <summary>
        /// 用于截取byte数组
        /// </summary>
        /// <param name="by">需截取的数组</param>
        /// <param name="start">截取数组起始位置</param>
        /// <param name="count">需截取的长度</param>
        /// <returns>截取完毕的数组</returns>
        static public byte[] CutByteArrayFromArrayToAnother(byte[] by, int start, int count)
        {
            byte[] cut = new byte[count];
            System.Buffer.BlockCopy(by, start, cut, 0, count);
            return cut;
        }

        static public byte[] Base64ToByteArray(string base64)
        {
            return Convert.FromBase64String(base64);
        }

        static public string ByteArrayToBase64(byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
        }
    }
}
