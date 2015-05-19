using System;
using System.Data;
using System.IO;
using Steam_Data_Collection.Networking;

namespace Steam_Data_Collection
{
    internal static class Program
    {
        /// <summary>
        /// Socket used to communicate with the clients
        /// </summary>
        private static readonly ServerSocket Socket = new ServerSocket();

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
        private static void Main(string[] args)
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
        private static void UpdateSettings()
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
            return (DataTable) temp.Select(msg);
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

        /// <summary>
        /// Sends a count command to the sql server
        /// </summary>
        /// <param name="msg">The statement to send to the server</param>
        public static string Count(string msg)
        {
            var temp = new SqlConnecter("db_steamdata");
            return temp.Count(msg).ToString();
        }
    }
}