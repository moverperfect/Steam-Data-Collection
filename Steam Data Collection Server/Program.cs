using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Steam_Data_Collection.Networking;

namespace Steam_Data_Collection
{
    class Program
    {
        private static ServerSocket Socket = new ServerSocket();

        public static SqlConnecter SqlDb = new SqlConnecter("db_steamdata");

        static void Main(string[] args)
        {
            Socket.Bind(8220);
            Socket.Listen(500);
            Socket.Accept();
            Console.ReadLine();
        }
    }
}
