using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml;
using Steam_Data_Collection_Client.Networking;
using Steam_Data_Collection_Client.Networking.Packets;
using Steam_Data_Collection_Client.Objects;

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
        /// Update the steam token of the program used to communicate with steam
        /// </summary>
        public static void UpdateSteamToken()
        {
            var msg = new StdData("", Program.HostId, 0, 2000);
            var rec = new StdData(CustomSocket.StartClient(msg.Data));
            if (rec.PacketType == 2000)
            {
                Program.SteamToken = rec.Text;
            }
        }

        /// <summary>
        /// Will cycle through all of the functions, updating what is needed
        /// </summary>
        public static void UpdateAll()
        {
            UpdateHostId();
            UpdateSteamToken();
            while (true)
            {
                Console.Clear();
                Thread.Sleep(1000);
                Console.Clear();
                Console.WriteLine("Server IP: " + Program.IpAddress.ToString() + "      Port: " + Program.Port.ToString() + "       HostId: " + Program.HostId);
                var packet = CustomSocket.StartClient(new StdData("", Program.HostId, 0, 2002).Data);
                UpdatePlayerSum(new ListOfId(packet).List);
            }
        }

        static void DiscoverPlayers()
        {
            
        }

        /// <summary>
        /// Get the steam ids from the server then check their summaries
        /// </summary>
        public static bool UpdatePlayerSum(List<ulong> listOfIds)
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

            // If we have not been given any ids then get some from the server
            if (listOfIds == null)
            {
                Console.WriteLine("Getting the steam ids we need to check from the server");
                listOfIds = new ListOfId(CustomSocket.StartClient(new StdData("", Program.HostId, 0, 2003).Data)).List;
            }

            // Exit if we have no id's
            if (listOfIds.Count == 0)
            {
                Console.WriteLine("No summaries to update");
                return false;
            }

            Console.WriteLine("List received of length " + listOfIds.Count);
            var uri =
                "http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key=" + Program.SteamToken + "&steamids=" + listOfIds[0];
            for (int i = 1; i < listOfIds.Count; i++)
            {
                uri += "," + listOfIds[i];
            }
            uri += "&format=xml";
            var s = new XmlReaderSettings {DtdProcessing = DtdProcessing.Ignore};
            var r = XmlReader.Create(uri,s);
            var people = new List<User>();
            while (r.Read())
            {
                switch (r.NodeType)
                {
                        case XmlNodeType.Element:
                        switch (r.Name)
                        {
                            case "player":
                                r.Read();
                                people.Add(new User());
                                break;
                        }
                        break;
                }
            }
            Console.ReadLine();
            return true;
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
