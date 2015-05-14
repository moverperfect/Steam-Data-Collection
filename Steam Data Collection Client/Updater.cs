using System;
using System.Threading;
using Steam_Data_Collection_Client.Networking;
using Steam_Data_Collection_Client.Networking.Packets;

namespace Steam_Data_Collection_Client
{
    static class Updater
    {
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

        public static void UpdateAll()
        {
            while (true)
            {
                Console.Clear();
                UpdateHostId();
                Thread.Sleep(1000);
                Console.Clear();
                Console.WriteLine("Server IP: " + Program.IpAddress.ToString() + "      Port: " + Program.Port.ToString() + "       HostId: " + Program.HostId);
                Console.ReadLine();
                UpdatePlayerSum();
            }
        }

        static void DiscoverPlayers()
        {
            
        }

        static void UpdatePlayerSum()
        {
            
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
