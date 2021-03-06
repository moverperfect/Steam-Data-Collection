﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Steam_Data_Collection_Client.Networking.Packets;

namespace Steam_Data_Collection
{
    internal static class DataDealer
    {
        /// <summary>
        /// A list of the current ids being scanned on the summary
        /// </summary>
        private static readonly List<CurrentScan> CurrSumList = new List<CurrentScan>();

        /// <summary>
        /// A list of the current ids being scanned on the game
        /// </summary>
        private static readonly List<CurrentScan> CurrGameList = new List<CurrentScan>();

        /// <summary>
        /// A list of all of the games that need to be scanned
        /// </summary>
        private static List<ulong> _gamesNeedScan = new List<ulong>();

        private static List<ulong> _sumNeedScan = new List<ulong>();

        /// <summary>
        /// Last time the game ids were updated
        /// </summary>
        private static DateTime _gameLastUpdate;

        private static DateTime _sumLastUpdate;

        /// <summary>
        /// A list of the current ids being scanned on the friend
        /// </summary>
        private static readonly List<CurrentScan> CurrFriendList = new List<CurrentScan>();

        /// <summary>
        /// Object for holding the state of a thread to allow other threads to pass
        /// </summary>
        private static readonly object GameUpdateLock = new object();

        private static readonly object SumUpdateLock = new object();

        /// <summary>
        /// Whether we are currently in the middle of updating 1000 peoples games
        /// </summary>
        private static bool _updateGames;

        /// <summary>
        /// Tries to update all the things, returns a packet of the first thing to be updated
        /// </summary>
        /// <param name="hostId">The host id of the machine who is asking</param>
        /// <returns>Returns a byte array containing a list of id's/url information plus packet type</returns>
        public static byte[] UpdateAll(int hostId)
        {
            // Get all of the ids that need their games updated
            var updategames = UpdateGames(false, hostId);

            // If there more than 1000 people to update then update, else false
            if (updategames.List.Count > 1000)
            {
                _updateGames = true;
            }
            else if (updategames.List.Count == 0)
            {
                _updateGames = false;
            }

            // If we are not updating games then check summary
            if (!_updateGames)
            {
                // Check sum and check if we need to update these
                var sum = UpdateSum(true, hostId);
                if (sum.List.Count > 0)
                {
                    return sum.Data;
                }
            }
            else
            {
                // Else update peoples games and mark the ones we are doing
                var game = UpdateGames(true, hostId);
                return game.List.Count > 0 ? game.Data : UpdateAll(hostId);

                // If we failed at updating games then call this function again
            }

            //var friend = UpdateFriends(true, hostId);

            //if (friend.List.Count > 0) return friend.Data;

            return new ListOfId(new List<ulong>(), 0, 2000).Data;
        }

        /// <summary>
        /// Return a list of ids that need to be updated in the order they need to be updated
        /// </summary>
        public static ListOfId UpdateSum(bool mark, int hostId)
        {
            Monitor.Enter(SumUpdateLock);

            if (_sumNeedScan.Count == 0 || _sumLastUpdate < DateTime.Now.AddSeconds(-60))
            {
                var dt =
                    Program.Select("SELECT PK_SteamId FROM tbl_user WHERE LastSummaryUpdate < NOW() - Interval " +
                                   Program.UpdateInterval +
                                   "  OR LastSummaryUpdate is Null ORDER BY LastSummaryUpdate;");
                _sumNeedScan = new List<ulong>();

                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    _sumNeedScan.Add((ulong) dt.Rows[i][0]);
                }

                _sumLastUpdate = DateTime.Now;
            }

            if (!mark)
            {
                return new ListOfId(_sumNeedScan, 0, 2003);
            }

            var listOfIds = new List<ulong>();

            foreach (var t in _sumNeedScan)
            {
                var scanned = CurrSumList.Any(currentScan => currentScan.SteamId == t);
                if (!scanned)
                {
                    listOfIds.Add(t);
                }
                if (listOfIds.Count == 100)
                {
                    break;
                }
            }

            // If we are marking these then remove all of ids currently being searched
            foreach (var t in CurrSumList)
            {
                listOfIds.Remove(t.SteamId);
            }

            // Mark all of the ids being searched as being searched
            foreach (var id in listOfIds)
            {
                CurrSumList.Add(new CurrentScan {HostId = hostId, SteamId = id, TimeOfScan = DateTime.Now});
            }

            Monitor.Exit(SumUpdateLock);

            // Return the list
            return new ListOfId(listOfIds, 0, 2003);
        }

        /// <summary>
        /// Get a list of the ids that need to be game updated
        /// </summary>
        /// <param name="mark">Whether to 'mark' the ids as being searched</param>
        /// <param name="hostId">The host id of the machine going to be searching</param>
        /// <returns>A list of the ids to be searched through</returns>
        public static ListOfId UpdateGames(bool mark, int hostId)
        {
            Monitor.Enter(GameUpdateLock);

            // If we need to scan again then do so
            if (_gamesNeedScan.Count == 0 || _gameLastUpdate < DateTime.Now.AddSeconds(-30))
            {
                FindGames();
            }

            var listOfIds = _gamesNeedScan;

            if (mark)
            {
                foreach (var t in CurrGameList.ToList())
                {
                    listOfIds.Remove(t.SteamId);
                }

                var l = 5;

                if (listOfIds.Count < 5)
                {
                    l = listOfIds.Count;
                }

                listOfIds = listOfIds.GetRange(0, l);

                foreach (var id in listOfIds)
                {
                    CurrGameList.Add(new CurrentScan {HostId = hostId, SteamId = id, TimeOfScan = DateTime.Now});
                    _gamesNeedScan.Remove(id);
                }
            }

            Monitor.Exit(GameUpdateLock);

            return new ListOfId(listOfIds, 0, 2004);
        }

        /// <summary>
        /// Grab all of the games from the server
        /// </summary>
        private static void FindGames()
        {
            var dt =
                Program.Select("SELECT PK_SteamID FROM tbl_user WHERE ((LastGameUpdate < NOW() - Interval " +
                               Program.UpdateInterval +
                               " ) OR LastGameUpdate is Null) AND VisibilityState = 1 AND (LastSummaryUpdate >= (NOW() - Interval " +
                               Program.UpdateInterval + ")) ORDER BY LastGameUpdate;");
            var listOfIds = new List<ulong>();

            for (var i = 0; i < dt.Rows.Count; i++)
            {
                listOfIds.Add((ulong) dt.Rows[i][0]);
            }

            _gamesNeedScan = listOfIds;

            _gameLastUpdate = DateTime.Now;
        }

        /// <summary>
        /// Gives a list of the steam ids that need friend updates
        /// </summary>
        /// <param name="mark">Whether to 'mark' the ids as being searched</param>
        /// <param name="hostId">The host id of the machine going to be searching</param>
        /// <returns>A list of ids to be searched through</returns>
        public static ListOfId UpdateFriends(bool mark, int hostId)
        {
            var dt =
                Program.Select("SELECT PK_SteamId FROM tbl_user WHERE (LastFriendUpdate < NOW() - Interval " +
                               Program.UpdateInterval +
                               ") OR LastFriendUpdate is Null AND VisibilityState = 1 ORDER BY LastFriendUpdate;");
            var listOfIds = new List<ulong>();

            for (var i = 0; i < dt.Rows.Count; i++)
            {
                listOfIds.Add((ulong) dt.Rows[i][0]);
            }

            if (mark)
            {
                foreach (var t in CurrFriendList.ToList())
                {
                    listOfIds.Remove(t.SteamId);
                }
            }
            var l = 5;

            if (listOfIds.Count < 5)
            {
                l = listOfIds.Count;
            }

            listOfIds = listOfIds.GetRange(0, l);

            if (mark)
            {
                foreach (var id in listOfIds)
                {
                    CurrFriendList.Add(new CurrentScan {HostId = hostId, SteamId = id, TimeOfScan = DateTime.Now});
                }
            }

            return new ListOfId(listOfIds, 0, 2006);
        }

        /// <summary>
        /// Returns a list of a steam user that has a game that needs a game name update
        /// </summary>
        /// <param name="hostId">The host id of the machine that requested the data</param>
        /// <returns>A list of 1 user that can update the names of games</returns>
        public static ListOfId UpdateGameNames(int hostId)
        {
            var dt =
                Program.Select(
                    "SELECT PK_SteamID FROM tbl_user RIGHT JOIN tbl_gcollection ON tbl_user.PK_SteamID = tbl_gcollection.FK_SteamID LEFT JOIN tbl_games ON tbl_gcollection.FK_AppID = tbl_games.PK_AppID WHERE PK_AppID is null GROUP BY FK_AppID LIMIT 1;");

            var listOfIds = new List<ulong>();

            for (var i = 0; i < dt.Rows.Count; i++)
            {
                listOfIds.Add((ulong) dt.Rows[i][0]);
            }

            return new ListOfId(listOfIds, 0, 2007);
        }

        /// <summary>
        /// Deals with the list of users from the client and adds it to the sql server
        /// </summary>
        /// <param name="tempList">The list of users info from the client</param>
        public static void DealWithSum(ListOfUsers tempList)
        {
            foreach (var user in tempList.List)
            {
                var update = "UPDATE tbl_user SET VisibilityState = " + ((user.VisibilityState) ? 1 : 0) +
                             ", UserName = '" +
                             ChangeString(user.UserName).Replace("\\", "\\\\")
                                 .Replace("'", "\\\'")
                                 .Replace("＇", "\\＇")
                                 .Replace("＼", "\\＼").Replace("ˈ", "\\ˈ").Replace("ˈ", "\\ˈ") + "', LastLogOff = '" +
                             user.LastLogOff.ToString("yyyy-MM-dd HH:mm:ss") + "', CustomURL = '" + user.CustomUrl +
                             "', LastSummaryUpdate = '" + user.LastSummaryUpdate.ToString("yyyy-MM-dd HH:mm:ss") + "'";

                if (user.VisibilityState)
                {
                    if (user.RealName != null)
                    {
                        update += ", RealName = '" + ChangeString(user.RealName).Replace("\\", "\\\\")
                            .Replace("'", "\\\'")
                            .Replace("＇", "\\＇")
                            .Replace("＼", "\\＼").Replace("ˈ", "\\ˈ").Replace("ˈ", "\\ˈ") + "'";
                    }

                    update += ", PrimaryClanID = '" + user.PrimaryClanId +
                              "', MemberSince = '" + user.MemberSince.ToString("yyyy-MM-dd HH:mm:ss") +
                              "', Location = '" +
                              user.Location +
                              "'";
                }

                update += " WHERE PK_SteamID = '" + user.SteamId + "';";

                Program.NonQuery(update);
            }

            Monitor.Enter(SumUpdateLock);

            foreach (var user in tempList.List)
            {
                for (var i = 0; i < CurrSumList.Count; i++)
                {
                    if (CurrSumList[i].SteamId == user.SteamId)
                    {
                        CurrSumList.RemoveAt(i);
                        break;
                    }
                }

                if (_sumNeedScan.Contains(user.SteamId))
                {
                    _sumNeedScan.Remove(user.SteamId);
                }
            }

            Monitor.Exit(SumUpdateLock);
        }

        /// <summary>
        /// Deals with the incoming data about games and parses it into the sql sever
        /// </summary>
        /// <param name="tempList">The user game information</param>
        public static void DealWithGames(ListOfUsers tempList)
        {
            foreach (var user in tempList.List)
            {
                var updateUser = "UPDATE tbl_user SET LastGameUpdate = '" +
                                 user.LastGameUpdate.ToString("yyyy-MM-dd HH:mm:ss") + "' WHERE PK_SteamId = " +
                                 user.SteamId +
                                 ";";
                var insertLink = "INSERT INTO tbl_gcollectionlink VALUES (" + user.SteamId + ", '" +
                                 user.LastGameUpdate.ToString("yyyy-MM-dd HH:mm:ss") + "');";
                var insertGCollection =
                    user.ListOfGames.Aggregate(
                        "INSERT INTO tbl_gcollection(`FK_SteamID`,`FK_TimeStamp`,`FK_AppID`,`MinsLast2Weeks`,`MinsOnRecord`) VALUES ",
                        (current, game) =>
                            current +
                            ("('" + user.SteamId + "', '" + user.LastGameUpdate.ToString("yyyy-MM-dd HH:mm:ss") + "', '" +
                             game.AppId + "', '" + game.Last2Weeks + "', '" + game.OnRecord + "'),"));

                if (user.ListOfGames.Count == 0)
                {
                    insertGCollection = "";
                }

                insertGCollection = insertGCollection.TrimEnd(',') + ";";

                Program.NonQuery(updateUser + insertLink + insertGCollection);
            }

            Monitor.Enter(GameUpdateLock);

            foreach (var user in tempList.List)
            {
                for (var i = 0; i < CurrGameList.Count; i++)
                {
                    if (CurrGameList[i].SteamId == user.SteamId)
                    {
                        CurrGameList.RemoveAt(i);
                        break;
                    }
                }
                if (_gamesNeedScan.Contains(user.SteamId))
                {
                    _gamesNeedScan.Remove(user.SteamId);
                }
            }

            Monitor.Exit(GameUpdateLock);
        }

        /// <summary>
        /// Deals with the incoming friend list
        /// </summary>
        /// <param name="tempList">A list of users containing the list of friends</param>
        public static void DealWithFriends(ListOfUsers tempList)
        {
            foreach (var user in tempList.List)
            {
                foreach (var friend in user.ListOfFriends)
                {
                    var user1 = user.SteamId;
                    var user2 = friend.SteamId;

                    if (user1 > user2)
                    {
                        var temp = user1;
                        user1 = user2;
                        user2 = temp;
                    }

                    var insert = "INSERT IGNORE INTO tbl_friend VALUES ('" + user1 + "','" + user2 + "','" +
                                 friend.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss") +
                                 "'); INSERT IGNORE INTO tbl_user SET PK_SteamID = '" + friend.SteamId + "';";
                    Program.NonQuery(insert);
                }
                Program.NonQuery("UPDATE tbl_user SET LastFriendUpdate = '" +
                                 user.LastFriendUpdate.ToString("yyyy-MM-dd HH:mm:ss") + "' WHERE PK_SteamID = '" +
                                 user.SteamId + "';");
            }
        }

        /// <summary>
        /// Deal with all of the given game names that are taken in
        /// </summary>
        /// <param name="tempList"></param>
        public static void DealWithGameNames(ListOfGames tempList)
        {
            var update = "";
            foreach (var gameHistory in tempList.List)
            {
                update += "INSERT IGNORE INTO tbl_games VALUES ('" + gameHistory.AppId + "','" +
                          ChangeString(gameHistory.Name).Replace("\\", "\\\\")
                              .Replace("'", "\\\'")
                              .Replace("＇", "\\＇")
                              .Replace("＼", "\\＼").Replace("ˈ", "\\ˈ").Replace("ˈ", "\\ˈ") + "');";
            }
            Program.NonQuery(update);
        }

        /// <summary>
        /// Changes a string into a 'latin1' string
        /// </summary>
        /// <param name="msg">The string to be changed</param>
        /// <returns>A latin1 parsed string</returns>
        private static string ChangeString(string msg)
        {
            var iso = Encoding.GetEncoding("ISO-8859-1");
            var utf8 = Encoding.UTF8;
            var utfBytes = utf8.GetBytes(msg);
            var isoBytes = Encoding.Convert(utf8, iso, utfBytes);
            return iso.GetString(isoBytes);
        }

        /// <summary>
        /// Holds the information about users we are currently scanning
        /// </summary>
        private struct CurrentScan
        {
            /// <summary>
            /// The Id of the user we are scanning
            /// </summary>
            public ulong SteamId { get; set; }

            /// <summary>
            /// The time that we start the scan
            /// </summary>
            public DateTime TimeOfScan { get; set; }

            /// <summary>
            /// The id of the host machine that is doing the scan
            /// </summary>
            public int HostId { get; set; }
        }
    }
}