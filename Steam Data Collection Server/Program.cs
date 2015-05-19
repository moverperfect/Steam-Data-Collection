using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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
        /// Steam token used to connect with steam
        /// </summary>
        public static String SteamToken;

        /// <summary>
        /// Stores the update interval to update statistics about users
        /// </summary>
        public static String UpdateInterval;

        /// <summary>
        /// Main entry point into the program, gets the token and starts listening
        /// </summary>
        static void Main(string[] args)
        {
            UpdateSettings();

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

        /// <summary>
        /// Updated the server settings
        /// </summary>
        static void UpdateSettings()
        {
            if (!File.Exists("settings.txt"))
            {
                Console.WriteLine("Please enter your steam token");
                SteamToken = Console.ReadLine();

                Console.WriteLine("Please enter the time interval you would like to update i.e 2 week");
                UpdateInterval = Console.ReadLine();

                File.WriteAllLines("settings.txt", new[] {SteamToken, UpdateInterval});
                return;
            }

            var settings = File.ReadAllLines("settings.txt");

            SteamToken = settings[0];
            UpdateInterval = settings[1];
        }

        /// <summary>
        /// Sends a select statement to the mysql sever
        /// </summary>
        /// <param name="msg">The statement to send to mysql</param>
        public static DataTable Select(String msg)
        {
            var temp = new SqlConnecter("db_steamdata");
            return (DataTable)temp.Select(msg);
        }

        /// <summary>
        /// Sends a nonquery to the sql sever
        /// </summary>
        /// <param name="msg">The statement to send to the server</param>
        public static void NonQuery(String msg)
        {
            var temp = new SqlConnecter("db_steamdata");
            temp.NonQuery(msg);
        }

    }
}
