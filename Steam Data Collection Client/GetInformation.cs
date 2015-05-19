using System;
using System.Threading;
using Steam_Data_Collection_Client.Networking;
using Steam_Data_Collection_Client.Networking.Packets;

namespace Steam_Data_Collection_Client
{
    /// <summary>
    /// Class that deals with giving information to the user
    /// Things to do:
    /// General statistics - Number in each table, the oldest current data we have
    /// User specific - Enter a username and get all their stats including a list of games
    /// They can then specify a game and get more details about that including a graph
    /// 
    /// Seven degrees - Enter two different usernames and get back the path to get between them and the route
    /// </summary>
    public static class GetInformation
    {
        public static void ShowGenStats()
        {
            while (true)
            {
                if (Program.HostId == 0)
                {
                    Console.Clear();
                    Updater.UpdateHostId();
                    Thread.Sleep(1000);
                }

                Console.Clear();

                Console.WriteLine("Server IP: " + Program.IpAddress + "      Port: " + Program.Port + "       HostId: " + Program.HostId);

                Console.WriteLine();

                Console.WriteLine("Getting the information from the server");

                var information = new StdData(CustomSocket.StartClient(new StdData("", Program.HostId, 0, 2050).Data)).Text;

                Console.WriteLine(information);

                Console.WriteLine("1. Refresh");
                Console.WriteLine("2. Exit");

                var o = Console.ReadLine();

                if (o == "1")
                {
                    continue;
                }
                break;
            }
        }
    }
}