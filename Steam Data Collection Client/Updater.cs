using System;
using System.Threading;
using Steam_Data_Collection_Client.Networking;
using Steam_Data_Collection_Client.Networking.Packets;

namespace Steam_Data_Collection_Client
{
    static class Updater
    {
        /// <summary>
        /// Asks the server for a new host id
        /// </summary>
        static void UpdateHostId()
        {
            var msg = new StdData("", Program.HostId, 0, 2001);
            var rec = new StdData(CustomSocket.StartClient(msg.Data));
            if (rec.PacketType == 2001)
            {
                ushort.TryParse(rec.Text, out Program.HostId);
            }

            Console.WriteLine("New host id has been assigned: " + Program.HostId);
        }

        /// <summary>
        /// Will cycle through all of the functions, updating what is needed
        /// </summary>
        public static void UpdateAll()
        {
            while (true)
            {
                Console.Clear();
                UpdateHostId();
                Thread.Sleep(1000);
                Console.Clear();
                Console.WriteLine("Server IP: " + Program.IpAddress.ToString() + "      Port: " + Program.Port.ToString() + "       HostId: " + Program.HostId);
                UpdatePlayerSum();
            }
        }

        static void DiscoverPlayers()
        {
            
        }

        /// <summary>
        /// Get the steam ids from the server then check their summaries
        /// </summary>
        static void UpdatePlayerSum()
        {
            if (Program.HostId == 0)
            {
                Console.Clear();
                UpdateHostId();
                Thread.Sleep(1000);
            }
            Console.Clear();
            Console.WriteLine("Server IP: " + Program.IpAddress.ToString() + "      Port: " + Program.Port.ToString() + "       HostId: " + Program.HostId);
            Console.WriteLine();
            Console.WriteLine("Getting the steam ids we need to check from the server");
            var listOfIds = new ListOfId(CustomSocket.StartClient(new StdData("", Program.HostId, 0, 2002).Data));
            Console.WriteLine("List received of length " + listOfIds.List.Count);
            Console.ReadLine();
        }

        static void UpdatePlayerFriend()
        {
            
        }

        static void UpdatePlayerGames()
        {
            
        }

        static void UpdatePlayerGroups()
        {
            
        }

        static void UpdateGroups()
        {
            
        }
    }
}
