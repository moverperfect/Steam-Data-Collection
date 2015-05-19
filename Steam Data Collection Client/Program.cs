using System;
using System.IO;
using System.Net;
using System.Threading;

namespace Steam_Data_Collection_Client
{
    internal static class Program
    {
        /// <summary>
        /// The ipaddress of the server machine
        /// </summary>
        public static IPAddress IpAddress;

        /// <summary>
        /// The port number for communication
        /// </summary>
        public static int Port;

        /// <summary>
        /// The host id of the machine, assigned by the server
        /// </summary>
        public static ushort HostId;

        /// <summary>
        /// The token used to communicate with steam
        /// </summary>
        public static String SteamToken;

        /// <summary>
        /// Entry point into the program
        /// </summary>
        private static void Main(string[] args)
        {
            // Get the info for the server
            GetServerInfo();

            while (true)
            {
                Console.Clear();
                Console.WriteLine("Server IP: " + IpAddress + "      Port: " + Port);
                Console.WriteLine();
                Console.WriteLine("Welcome to the steam data collection client!!");
                Console.WriteLine("Please choose one of these options");
                Console.WriteLine("1. Be A Slave");
                Console.WriteLine("2. Get Information");
                Console.WriteLine("3. Options");
                Console.WriteLine("4. Exit");

                var option = Console.ReadLine();

                switch (option)
                {
                    case "1":
                        SlaveMenu();
                        break;

                    case "2":
                        GetInformationMenu();
                        break;

                    case "3":
                        OptionsMenu();
                        break;

                    case "4":
                        Environment.Exit(0);
                        break;
                }
            }
        }

        /// <summary>
        /// Displays and controls the "Be a slave" menu
        /// </summary>
        private static void SlaveMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Server IP: " + IpAddress + "      Port: " + Port);
                Console.WriteLine();
                Console.WriteLine("You are a slave, do you care what you do?");
                Console.WriteLine("1. No, just update \"Stuff\"");
                Console.WriteLine("2. Update player summaries");
                Console.WriteLine("3. Update player games");
                Console.WriteLine("4. Update player groups");
                Console.WriteLine("5. Update group's");
                Console.WriteLine("6. Exit");

                var option = Console.ReadLine();

                switch (option)
                {
                    case "1":
                        Updater.UpdateAll();
                        break;

                    case "2":
                        while (Console.KeyAvailable == false)
                        {
                            if (Updater.UpdatePlayerSum(null) == false)
                            {
                                Console.WriteLine("There are no users to update trying again in 30 seconds");
                                Thread.Sleep(30000);
                            }
                            Thread.Sleep(500);
                        }
                        break;

                    case "3":
                        while (Console.KeyAvailable == false)
                        {
                            if (Updater.UpdatePlayerGames(null) == false)
                            {
                                Console.WriteLine("There are no users to update trying again in 30 seconds");
                                Thread.Sleep(30000);
                            }
                            Thread.Sleep(500);
                        }
                        break;

                    case "4":
                        break;

                    case "5":
                        break;

                    case "6":
                        break;

                    case "7":
                        return;
                }
            }
        }

        /// <summary>
        /// Displays and controls the "Get Information" menu
        /// </summary>
        private static void GetInformationMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Server IP: " + IpAddress + "      Port: " + Port);
                Console.WriteLine();
                Console.WriteLine("This has not been implemented yet, sorry");
                Console.WriteLine("1. General statistics");
                Console.WriteLine("2. Exit");

                var option = Console.ReadLine();

                switch (option)
                {
                    case "1":
                        GetInformation.ShowGenStats();
                        break;

                    case "2":
                        return;
                }
            }
        }

        /// <summary>
        /// Displays and controls the options menu for the user
        /// </summary>
        private static void OptionsMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Server IP: " + IpAddress + "      Port: " + Port);
                Console.WriteLine();
                Console.WriteLine("Welcome to the options menu, please select one of the options");
                Console.WriteLine("1. Change the server settings");
                Console.WriteLine("2. Exit");

                var option = Console.ReadLine();

                switch (option)
                {
                    case "1":
                        WriteServerInfo();
                        break;

                    case "2":
                        return;
                }
            }
        }

        /// <summary>
        /// Get the serverinfo from the file
        /// </summary>
        private static void GetServerInfo()
        {
            while (!File.Exists(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                "/SteamDataCollection/serverInfo.txt"))
            {
                WriteServerInfo();
            }

            var serverInfo = File.ReadAllLines(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                               "/SteamDataCollection/serverInfo.txt");

            if (!IPAddress.TryParse(serverInfo[0], out IpAddress))
            {
                Console.WriteLine("Unable to parse the server ip address. Skipping");
                Thread.Sleep(500);
                return;
            }

            if (!int.TryParse(serverInfo[1], out Port))
            {
                Console.WriteLine("Unable to parse the port number. Skipping");
                Thread.Sleep(500);
            }
        }

        /// <summary>
        /// Write the information to the file in AppData
        /// </summary>
        private static void WriteServerInfo()
        {
            Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                      "/SteamDataCollection/");

            IPAddress ipAddress = null;
            var port = 0;
            while (ipAddress == null)
            {
                Console.WriteLine("Please enter the ip address of the server machine");
                IPAddress.TryParse(Console.ReadLine(), out ipAddress);
            }

            while (port == 0)
            {
                Console.WriteLine("Please enter the port number to contact the server with");
                int.TryParse(Console.ReadLine(), out port);
            }

            File.WriteAllLines(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                               "/SteamDataCollection/serverInfo.txt", new[] {ipAddress.ToString(), port.ToString()});

            IpAddress = ipAddress;
            Port = port;
        }
    }
}