using System;
using System.CodeDom.Compiler;
using System.Threading;
using Steam_Data_Collection_Client.Networking;
using Steam_Data_Collection_Client.Networking.Packets;
using Steam_Data_Collection_Client.Objects;

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
    internal static class GetInformation
    {
        /// <summary>
        /// Shows the general statistics for the server(The total stats for the tables)
        /// </summary>
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

                Console.WriteLine("Server IP: " + Program.IpAddress + "      Port: " + Program.Port + "       HostId: " +
                                  Program.HostId);

                Console.WriteLine();

                Console.WriteLine("Getting the information from the server");

                var information =
                    new StdData(CustomSocket.StartClient(new StdData("", Program.HostId, 0, 2050).Data)).Text;

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

        /// <summary>
        /// Shows the general player statistics and gives options
        /// </summary>
        public static void ShowPlayerStats()
        {
            Console.Clear();

            while (true)
            {
                if (Program.HostId == 0)
                {
                    Console.Clear();
                    Updater.UpdateHostId();
                    Thread.Sleep(1000);
                }

                Console.WriteLine("Server IP: " + Program.IpAddress + "      Port: " + Program.Port + "       HostId: " +
                                  Program.HostId);

                Console.WriteLine();

                Console.WriteLine("Please enter either your username or steam id");

                var player = Console.ReadLine();

                var playerInfo = new ListOfUsers(CustomSocket.StartClient(new StdData(player, Program.HostId, 0, 2051).Data));

                var user = new User();

                switch (playerInfo.List.Count)
                {
                    case 0:
                        Console.WriteLine("Username/Id not found, would you like to add it to our database");
                        Console.WriteLine("1. Yes");
                        Console.WriteLine("2. Try Again");
                        Console.WriteLine("3. Exit");
                        var option = Console.ReadLine();
                        switch (option)
                        {
                            case "1":
                                UInt64 steamId;
                                while (!UInt64.TryParse(player, out steamId))
                                {
                                    Console.WriteLine("Please re-enter the steam id or type e to exit");
                                    player = Console.ReadLine();
                                    if (player == "e")
                                    {
                                        return;
                                    }
                                }
                                CustomSocket.StartClient(new StdData(steamId.ToString(), Program.HostId, 0, 4000).Data);
                                Console.WriteLine(
                                    "The user has been entered into the database, may take a little while to start getting information about the new user");
                                Console.WriteLine("Returning in 5 seconds");
                                break;

                            case "2":
                                continue;

                            case "3":
                                return;
                        }
                        break;

                    case 1:
                        user = playerInfo.List[0];
                        break;

                    default:
                        Console.WriteLine("Multiple users have been detected, please select the one that you would like");
                        for (int i = 0; i < playerInfo.List.Count; i++)
                        {
                            Console.WriteLine("{0}. {1} {2}", i + 1, playerInfo.List[i].SteamId,
                                playerInfo.List[i].UserName);
                        }

                        var o = Console.ReadLine();
                        Int16 id = 0;
                        if (Int16.TryParse(o, out id))
                        {
                            user = playerInfo.List[id - 1];
                        }
                        break;
                }
                Console.SetBufferSize(80, 1000);
                Console.Clear();
                Console.WriteLine("Steam ID:            {0}", user.SteamId);
                Console.WriteLine("UserName:            {0}", user.UserName);
                Console.WriteLine("Profile URL:         http://steamcommunity.com/{0}", user.CustomUrl == "/" ? "profiles/" +user.SteamId.ToString() : "id/" + user.CustomUrl);
                Console.WriteLine("VisibilityState:     {0}", user.VisibilityState);
                if (user.VisibilityState)
                {
                    Console.WriteLine("Location:            {0}", user.Location);
                    Console.WriteLine("Real Name:           {0}", user.RealName);
                    Console.WriteLine("Member since:        {0}", user.MemberSince);
                    Console.WriteLine("Last Game Update:    {0}", user.LastGameUpdate);
                    Console.WriteLine("Last Friend Update:  {0}", user.LastFriendUpdate);
                }
                Console.WriteLine("Last Summary Update: {0}", user.LastSummaryUpdate);
                Console.WriteLine("Last Log Off:        {0}", user.LastLogOff);
                if (user.ListOfGames.Count > 0)
                {
                    Console.WriteLine("List of games:");
                    int i;
                    for (i = 0; i < user.ListOfGames.Count; i++)
                    {
                        Console.WriteLine("{0}. {1}", i + 1, user.ListOfGames[i].Name ?? user.ListOfGames[i].AppId.ToString());
                    }
                    Console.WriteLine("{0}. Exit", i+1);
                    
                }
                else
                {
                    Console.WriteLine("1. Exit");
                }

                // Do all of the menu stuff
                if(Console.ReadLine() == "1")
                    return;
                Console.ReadLine();
            }
        }
    }
}