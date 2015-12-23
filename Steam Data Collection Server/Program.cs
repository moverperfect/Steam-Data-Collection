using System;
using System.Data;
using System.IO;
using Steam_Data_Collection.Networking;

namespace Steam_Data_Collection
{
    internal static class Program
    {

        /// <summary>
        /// Steam token used to connect with steam
        /// </summary>
        public static String SteamToken;

        /// <summary>
        /// Stores the update interval to update statistics about users
        /// </summary>
        public static String UpdateInterval;

        /// <summary>
        /// Stores the sql server address
        /// </summary>
        public static String ServerAddress;

        /// <summary>
        /// Stores the username for the sql server
        /// </summary>
        public static String Username;

        /// <summary>
        /// Stores the password for the sql server
        /// </summary>
        public static String Password;

        /// <summary>
        /// Main entry point into the program, gets the token and starts listening
        /// </summary>
        private static void Main(string[] args)
        {
            UpdateSettings();

            ServerSocket.StartListening();
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

                Console.WriteLine("Please enter the sql server address");
                ServerAddress = Console.ReadLine();

                Console.WriteLine("Please enter the sql server username");
                Username = Console.ReadLine();
                
                Console.WriteLine("Please enter the sql server password");
                Password = Console.ReadLine();
                
                File.WriteAllLines("settings.txt", new[] {SteamToken, UpdateInterval, ServerAddress, Username, Password});
                return;
            }

            var settings = File.ReadAllLines("settings.txt");

            SteamToken = settings[0];
            UpdateInterval = settings[1];
            ServerAddress = settings[2];
            Username = settings[3];
            Password = settings[4];
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