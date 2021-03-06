﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Xml;
using Steam_Data_Collection_Client.Networking;
using Steam_Data_Collection_Client.Networking.Packets;
using Steam_Data_Collection_Client.Objects;

namespace Steam_Data_Collection_Client
{
    internal static class Updater
    {
        /// <summary>
        /// Asks the server for a new host id
        /// </summary>
        public static void UpdateHostId()
        {
            var msg = new StdData("", Program.HostId, 2001);
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
        private static void UpdateSteamToken()
        {
            var msg = new StdData("", Program.HostId, 2000);
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
            Console.Clear();
            while (Console.KeyAvailable == false)
            {
                Thread.Sleep(1000);
                Console.WriteLine("Server IP: " + Program.IpAddress + "      Port: " + Program.Port + "       HostId: " +
                                  Program.HostId);
                var packet = CustomSocket.StartClient(new StdData("", Program.HostId, 2002).Data);
                
                switch (new StdData(packet).PacketType)
                {
                    case 2003:
                        Console.WriteLine("Updating player summaries");
                        UpdatePlayerSum(new ListOfId(packet).List);
                        break;

                    case 2004:
                        Console.WriteLine("Updating player games");
                        UpdatePlayerGames(new ListOfId(packet).List);
                        break;

                    case 2006:
                        Console.WriteLine("Updating player friends");
                        UpdatePlayerFriends(new ListOfId(packet).List);
                        break;

                    default:
                        Console.WriteLine("Nothing to update, waiting 30 seconds");
                        Thread.Sleep(30000);
                        break;
                }
            }
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

                Console.Clear();
                Console.WriteLine("Server IP: " + Program.IpAddress + "      Port: " + Program.Port + "       HostId: " +
                                  Program.HostId);
                Console.WriteLine();
            }

            if (Program.SteamToken == null)
            {
                UpdateSteamToken();
            }

            // If we have not been given any ids then get some from the server
            if (listOfIds == null)
            {
                Console.WriteLine("Getting the steam ids we need to check from the server");
                listOfIds = new ListOfId(CustomSocket.StartClient(new StdData("", Program.HostId, 2003).Data)).List;
            }

            // Exit if we have no id's
            if (listOfIds.Count == 0)
            {
                Console.WriteLine("No summaries to update");
                return false;
            }

            Console.WriteLine("List received of length " + listOfIds.Count);
            var uri =
                "http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key=" + Program.SteamToken +
                "&steamids=" + listOfIds[0];
            for (var i = 1; i < listOfIds.Count; i++)
            {
                uri += "," + listOfIds[i];
            }
            uri += "&format=xml";

            Console.WriteLine("Getting the information from steam");

            var people = new List<User>();
            try
            {
                var s = new XmlReaderSettings {DtdProcessing = DtdProcessing.Ignore};
                var r = XmlReader.Create(uri, s);
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
                                    people[people.Count - 1].LastSummaryUpdate = DateTime.Now;
                                    break;

                                case "steamid":
                                    r.Read();
                                    people[people.Count - 1].SteamId = UInt64.Parse(r.Value);
                                    break;

                                case "communityvisibilitystate":
                                    r.Read();
                                    var v = r.Value == "3";
                                    people[people.Count - 1].VisibilityState = v;
                                    break;

                                case "personaname":
                                    r.Read();
                                    people[people.Count - 1].UserName = r.Value;
                                    break;

                                case "lastlogoff":
                                    r.Read();
                                    people[people.Count - 1].LastLogOff =
                                        new DateTime(1970, 1, 1).AddSeconds(double.Parse(r.Value));
                                    break;

                                case "profileurl":
                                    r.Read();
                                    people[people.Count - 1].CustomUrl = r.Value.Split('/')[3] == "id"
                                        ? r.Value.Split('/')[4]
                                        : "/";
                                    break;

                                case "realname":
                                    r.Read();
                                    people[people.Count - 1].RealName = r.Value;
                                    break;

                                case "primaryclanid":
                                    r.Read();
                                    people[people.Count - 1].PrimaryClanId = UInt64.Parse(r.Value);
                                    break;

                                case "timecreated":
                                    r.Read();
                                    people[people.Count - 1].MemberSince =
                                        new DateTime(1970, 1, 1).AddSeconds(double.Parse(r.Value));
                                    break;

                                case "loccountrycode":
                                    r.Read();
                                    people[people.Count - 1].Location = r.Value;
                                    break;
                            }
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                people.RemoveAt(people.Count - 1);
            }

            Console.WriteLine("Sending the information back to the server");

            CustomSocket.StartClient(new ListOfUsers(people, Program.HostId, 3003).Data);

            return true;
        }

        /// <summary>
        /// Gets the information from steam to update the games for users
        /// </summary>
        /// <param name="listOfIds">Can be null also can enter ids</param>
        /// <returns>True or false as to if the server has no more</returns>
        public static bool UpdatePlayerGames(List<ulong> listOfIds)
        {
            if (Program.HostId == 0)
            {
                Console.Clear();
                UpdateHostId();
                Thread.Sleep(1000);

                Console.Clear();
                Console.WriteLine("Server IP: " + Program.IpAddress + "      Port: " + Program.Port + "       HostId: " +
                                  Program.HostId);
                Console.WriteLine();
            }

            if (Program.SteamToken == null)
            {
                UpdateSteamToken();
            }

            // If we have not been given any ids then get some from the server
            if (listOfIds == null)
            {
                Console.WriteLine("Getting the steam ids we need to check from the server");
                listOfIds = new ListOfId(CustomSocket.StartClient(new StdData("", Program.HostId, 2004).Data)).List;
            }

            // Exit if we have no id's
            if (listOfIds.Count == 0)
            {
                Console.WriteLine("No games to update");
                return false;
            }

            Console.WriteLine("List received of length " + listOfIds.Count);

            var list = new List<User>();

            foreach (var listOfId in listOfIds)
            {
                var uri =
                    "http://api.steampowered.com/IPlayerService/GetOwnedGames/v0001/?key=" + Program.SteamToken +
                    "&steamid=" + listOfId
                    + "&include_played_free_games=1&format=xml";

                Console.WriteLine("Getting the information from steam");

                try
                {
                    var s = new XmlReaderSettings {DtdProcessing = DtdProcessing.Ignore};
                    var r = XmlReader.Create(uri, s);

                    var temp = new User {SteamId = listOfId, LastGameUpdate = DateTime.Now};

                    var tempGame = new GameHistory();

                    while (r.Read())
                    {
                        switch (r.NodeType)
                        {
                            case XmlNodeType.Element:
                                switch (r.Name)
                                {
                                    case "message":
                                        tempGame = new GameHistory();
                                        break;

                                    case "appid":
                                        r.Read();
                                        tempGame.AppId = Convert.ToInt32(r.Value);
                                        break;

                                    case "playtime_forever":
                                        r.Read();
                                        tempGame.OnRecord = Convert.ToInt32(r.Value);
                                        break;

                                    case "playtime_2weeks":
                                        r.Read();
                                        tempGame.Last2Weeks = Convert.ToInt32(r.Value);
                                        break;
                                }
                                break;

                            case XmlNodeType.EndElement:
                                switch (r.Name)
                                {
                                    case "message":
                                        temp.ListOfGames.Add(tempGame);
                                        break;
                                }
                                break;
                        }
                    }

                    list.Add(temp);
                }
                catch(Exception exception)
                {
                    Console.WriteLine(exception.Message);
                }
            }

            Console.WriteLine("Sending the information back to the server");

            CustomSocket.StartClient(new ListOfUsers(list, Program.HostId, 3004).Data);

            return true;
        }

        /// <summary>
        /// Updates a list of players friends
        /// </summary>
        /// <param name="listOfIds">A list of players, can be null</param>
        /// <returns>Were there any from the server</returns>
        public static bool UpdatePlayerFriends(List<ulong> listOfIds)
        {
            if (Program.HostId == 0)
            {
                Console.Clear();
                UpdateHostId();
                Thread.Sleep(1000);

                Console.Clear();
                Console.WriteLine("Server IP: " + Program.IpAddress + "      Port: " + Program.Port + "       HostId: " +
                                  Program.HostId);
                Console.WriteLine();
            }

            if (Program.SteamToken == null)
            {
                UpdateSteamToken();
            }
            
            // If we have not been given any ids then get some from the server
            if (listOfIds == null)
            {
                Console.WriteLine("Getting the steam ids we need to check from the server");
                listOfIds = new ListOfId(CustomSocket.StartClient(new StdData("", Program.HostId, 2006).Data)).List;
            }

            // Exit if we have no id's
            if (listOfIds.Count == 0)
            {
                Console.WriteLine("No users to update");
                return false;
            }

            Console.WriteLine("List received of length " + listOfIds.Count);

            var list = new List<User>();

            foreach (var listOfId in listOfIds)
            {
                var uri =
                    "http://api.steampowered.com/ISteamUser/GetFriendList/v0001/?key=" + Program.SteamToken +
                    "&steamid=" + listOfId
                    + "&relationship=friend&format=xml";

                Console.WriteLine("Getting the information from steam");
                try
                {
                    var s = new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore };
                    var r = XmlReader.Create(uri, s);

                    var temp = new User { SteamId = listOfId, LastFriendUpdate = DateTime.Now };

                    var tempFriend = new Friend();

                    while (r.Read())
                    {
                        switch (r.NodeType)
                        {
                            case XmlNodeType.Element:
                                switch (r.Name)
                                {
                                    case "steamid":
                                        r.Read();
                                        tempFriend = new Friend { SteamId = Convert.ToUInt64(r.Value) };
                                        break;

                                    case "friend_since":
                                        r.Read();
                                        tempFriend.TimeStamp = new DateTime(1970, 1, 1).AddSeconds(double.Parse(r.Value));
                                        break;
                                }
                                break;

                            case XmlNodeType.EndElement:
                                switch (r.Name)
                                {
                                    case "friend":
                                        temp.ListOfFriends.Add(tempFriend);
                                        break;
                                }
                                break;
                        }
                    }

                    list.Add(temp);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            Console.WriteLine("Sending the information back to the server");

            CustomSocket.StartClient(new ListOfUsers(list, Program.HostId, 3006).Data);

            return true;
        }

        /// <summary>
        /// Updates the names of games
        /// </summary>
        /// <param name="listOfIds">A list of the ids of the game</param>
        /// <returns>Were there any games that need to be updated</returns>
        public static bool UpdateGameNames(List<ulong> listOfIds)
        {
            if (Program.HostId == 0)
            {
                Console.Clear();
                UpdateHostId();
                Thread.Sleep(1000);

                Console.Clear();
                Console.WriteLine("Server IP: " + Program.IpAddress + "      Port: " + Program.Port + "       HostId: " +
                                  Program.HostId);
                Console.WriteLine();
            }

            if (Program.SteamToken == null)
            {
                UpdateSteamToken();
            }

            // If we have not been given any ids then get some from the server
            if (listOfIds == null)
            {
                Console.WriteLine("Getting the game ids we need to check from the server");
                listOfIds = new ListOfId(CustomSocket.StartClient(new StdData("", Program.HostId, 2007).Data)).List;
            }

            // Exit if we have no id's
            if (listOfIds.Count == 0)
            {
                Console.WriteLine("No games to update");
                return false;
            }

            Console.WriteLine("List received of length " + listOfIds.Count);

            var list = new List<GameHistory>();

            foreach (var listOfId in listOfIds)
            {
                var uri =
                    "http://api.steampowered.com/IPlayerService/GetOwnedGames/v0001/?key=" + Program.SteamToken +
                    "&steamid=" + listOfId
                    + "&include_played_free_games=1&include_appinfo=1&format=xml";

                Console.WriteLine("Getting the information from steam");
                try
                {
                    var s = new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore };
                    var r = XmlReader.Create(uri, s);

                    var temp = new GameHistory();

                    while (r.Read())
                    {
                        switch (r.NodeType)
                        {
                            case XmlNodeType.Element:
                                switch (r.Name)
                                {
                                    case "appid":
                                        r.Read();
                                        temp = new GameHistory();
                                        temp.AppId = Convert.ToInt32(r.Value);
                                        break;

                                    case "name":
                                        r.Read();
                                        temp.Name = r.Value;
                                        list.Add(temp);
                                        continue;
                                }
                                break;
                        }
                    }

                    list.Add(temp);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            Console.WriteLine("Sending the information back to the server");

            CustomSocket.StartClient(new ListOfGames(list, Program.HostId, 3007).Data);

            return true;
        }
    }
}