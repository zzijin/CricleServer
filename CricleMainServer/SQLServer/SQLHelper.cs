using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CricleMainServer.SQLServer
{
    class SQLHelper
    {
        //从网站的配置文件中读取数据库连接字符串
        static string cnstr = SQLConfiguration.Constr;

        /// <summary>
        /// 执行增、删、改的SQL命令或存储过程，返回受影响的行数
        /// </summary>
        /// <returns></returns>
        public static int ExecuteNonQuery(string sql, CommandType type, SqlParameter[] p)
        {
            int r = 0;
            using (SqlConnection cn = new SqlConnection(cnstr))
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = cn;
                cmd.CommandText = sql;
                cmd.CommandType = type;
                if (p != null)
                    cmd.Parameters.AddRange(p);
                r = cmd.ExecuteNonQuery();
            }
            return r;
        }

        /// <summary>
        /// 执行查询的SQL命令或存储过程，返回DataReader对象
        /// </summary>
        /// <returns></returns>
        public static SqlDataReader ExecuteReader(string sql, CommandType type, SqlParameter[] p)
        {
            SqlDataReader dr;
            SqlConnection cn = new SqlConnection(cnstr);
            cn.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = cn;
            cmd.CommandText = sql;
            cmd.CommandType = type;
            if (p != null)
            {
                cmd.Parameters.AddRange(p);
            }
            dr = cmd.ExecuteReader();
            return dr;
        }

        /// <summary>
        /// 执行查询的SQL命令或存储过程，返回一个值object
        /// </summary>
        /// <returns></returns>
        public static object ExecuteScalar(string sql, CommandType type, SqlParameter[] p)
        {
            object o;
            using (SqlConnection cn = new SqlConnection(cnstr))
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = cn;
                cmd.CommandText = sql;
                cmd.CommandType = type;
                if (p != null)
                {
                    cmd.Parameters.AddRange(p);
                }
                o = cmd.ExecuteScalar();
            }
            return o;
        }

        /// <summary>
        /// 执行查询的SQL命令或存储过程，返回数据集DataSet
        /// </summary>
        /// <returns></returns>
        public static DataSet ExecuteDataSet(string sql, CommandType type, SqlParameter[] p)
        {
            DataSet ds = new DataSet();
            using (SqlConnection cn = new SqlConnection(cnstr))
            {
                cn.Open();
                SqlDataAdapter da = new SqlDataAdapter();
                da.SelectCommand = new SqlCommand();
                da.SelectCommand.Connection = cn;
                da.SelectCommand.CommandText = sql;
                da.SelectCommand.CommandType = type;
                if (p != null)
                {
                    da.SelectCommand.Parameters.AddRange(p);
                }
                da.Fill(ds);
            }
            return ds;
        }
    }
}
