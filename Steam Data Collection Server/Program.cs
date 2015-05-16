using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Steam_Data_Collection.Networking;

namespace Steam_Data_Collection
{
    class Program
    {
        private static ServerSocket Socket = new ServerSocket();

        public static SqlConnecter SqlDb = new SqlConnecter("db_steamdata");

        public static String SteamToken;

        static void Main(string[] args)
        {
            SteamToken = "XX";

            Socket.Bind(8220);
            Socket.Listen(500);
            Socket.Accept();
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Listening..");
                Thread.Sleep(10000);
            }
        }

        static int NoSumUpdate()
        {
            var dt = (DataTable)Program.SqlDb.Select("SELECT PK_SteamId FROM tbl_user WHERE LastSummaryUpdate < NOW() - Interval 1 week  OR LastSummaryUpdate is Null ORDER BY LastSummaryUpdate;");
            return dt.Rows.Count;
        }
    }
}
