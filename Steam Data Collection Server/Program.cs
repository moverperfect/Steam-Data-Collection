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
        /// <summary>
        /// Socket used to communicate with the clients
        /// </summary>
        private static ServerSocket Socket = new ServerSocket();

        /// <summary>
        /// Sql connecter used to connect to mysql
        /// </summary>
        public static SqlConnecter SqlDb = new SqlConnecter("db_steamdata");

        /// <summary>
        /// Steam token used to connect with steam
        /// </summary>
        public static String SteamToken;

        /// <summary>
        /// Main entry point into the program, gets the token and starts listening
        /// </summary>
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
                Console.ReadLine();
            }
        }

    }
}
