using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
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
        /// A list of the current ids being scanned on the friend
        /// </summary>
        private static readonly List<CurrentScan> CurrFriendList = new List<CurrentScan>(); 

        /// <summary>
        /// Tries to update all the things, returns a packet of the first thing to be updated
        /// </summary>
        /// <param name="hostId">The host id of the machine who is asking</param>
        /// <returns>Returns a byte array containing a list of id's/url information plus packet type</returns>
        public static byte[] UpdateAll(int hostId)
        {
            var sum = UpdateSum(true, hostId);
            if (sum.List.Count > 0)
            {
                return sum.Data;
            }

            var game = UpdateGames(true, hostId);
            if (game.List.Count > 0)
            {
                return game.Data;
            }

            var friend = UpdateFriends(true, hostId);

            if (friend.List.Count > 0) return friend.Data;

            return new ListOfId(new List<ulong>(), 0, 2000).Data;
        }

        /// <summary>
        /// Return a list of ids that need to be updated in the order they need to be updated
        /// </summary>
        public static ListOfId UpdateSum(bool mark, int hostId)
        {
            var dt =
                Program.Select("SELECT PK_SteamId FROM tbl_user WHERE LastSummaryUpdate < NOW() - Interval " +
                               Program.UpdateInterval +
                               "  OR LastSummaryUpdate is Null ORDER BY LastSummaryUpdate;");
            var listOfIds = new List<UInt64>();

            for (var i = 0; i < dt.Rows.Count; i++)
            {
                listOfIds.Add((UInt64) dt.Rows[i][0]);
            }

            if (mark)
            {
                foreach (var t in CurrSumList)
                {
                    listOfIds.Remove(t.SteamId);
                }
            }

            var l = 100;

            if (listOfIds.Count < 100)
            {
                l = listOfIds.Count;
            }

            listOfIds = listOfIds.GetRange(0, l);

            if (mark)
            {
                foreach (var id in listOfIds)
                {
                    CurrSumList.Add(new CurrentScan {HostId = hostId, SteamId = id, TimeOfScan = DateTime.Now});
                }
            }


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
            var dt =
                Program.Select("SELECT PK_SteamId FROM tbl_user WHERE (LastGameUpdate < NOW() - Interval " +
                               Program.UpdateInterval +
                               ") OR LastGameUpdate is Null AND VisibilityState = 1 ORDER BY LastGameUpdate;");
            var listOfIds = new List<UInt64>();

            for (var i = 0; i < dt.Rows.Count; i++)
            {
                listOfIds.Add((UInt64) dt.Rows[i][0]);
            }

            if (mark)
            {
                foreach (var t in CurrGameList.ToList())
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
                    CurrGameList.Add(new CurrentScan {HostId = hostId, SteamId = id, TimeOfScan = DateTime.Now});
                }
            }

            return new ListOfId(listOfIds, 0, 2004);
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
            var listOfIds = new List<UInt64>();

            for (var i = 0; i < dt.Rows.Count; i++)
            {
                listOfIds.Add((UInt64)dt.Rows[i][0]);
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
                    CurrFriendList.Add(new CurrentScan { HostId = hostId, SteamId = id, TimeOfScan = DateTime.Now });
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

            var listOfIds = new List<UInt64>();

            for (var i = 0; i < dt.Rows.Count; i++)
            {
                listOfIds.Add((UInt64)dt.Rows[i][0]);
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
                var update = "UPDATE tbl_User SET VisibilityState = " + ((user.VisibilityState) ? 1 : 0) +
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
            }
        }

        /// <summary>
        /// Deals with the incoming data about games and parses it into the sql sever
        /// </summary>
        /// <param name="tempList">The user game information</param>
        public static void DealWithGames(ListOfUsers tempList)
        {
            foreach (var user in tempList.List)
            {
                var updateUser = "UPDATE tbl_User SET LastGameUpdate = '" +
                                 user.LastGameUpdate.ToString("yyyy-MM-dd HH:mm:ss") + "' WHERE PK_SteamId = " +
                                 user.SteamId +
                                 ";";
                var insertLink = "INSERT INTO tbl_GCollectionLink VALUES (" + user.SteamId + ", '" +
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
            }
        }

        /// <summary>
        /// Deals with the incoming friend list
        /// </summary>
        /// <param name="tempList">A list of users containing the list of friends</param>
        public static void DealWithFriends(ListOfUsers tempList)
        {
            foreach (var user in tempList.List)
            {
                
                var dt =
                    Program.Select(
                        "SELECT tbl_friends.PK_UserID, t.PK_UserID AS UserID2 FROM db_steamdata.tbl_friends INNER JOIN ( SELECT * FROM tbl_friends ) t ON tbl_friends.PK_FriendID = t.PK_FriendID WHERE tbl_friends.PK_UserID != t.PK_UserID AND tbl_friends.PK_UserID = '" +
                        user.SteamId + "';");

                foreach (DataRow row in dt.Rows)
                {
                    for (var index = 0; index < user.ListOfFriends.Count; index++)
                    {
                        var friend = user.ListOfFriends[index];

                        if (Convert.ToUInt64(row[1]) == friend.SteamId)
                        {
                            user.ListOfFriends.RemoveAt(index);
                            break;
                        }
                    }
                }
                foreach (var friend in user.ListOfFriends)
                {
                    var insert = "INSERT INTO tbl_friends(PK_UserID) VALUES ('" + user.SteamId +
                              "'); SET @friendid = LAST_INSERT_ID(); INSERT INTO tbl_friends VALUES (@friendid,'" +
                              friend.SteamId + "');INSERT INTO tbl_frienddate VALUES (@friendid, '" + friend.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss") +
                              "'); INSERT IGNORE INTO tbl_user SET PK_SteamID = '" + friend.SteamId + "';";
                    Program.NonQuery(insert);
                }
                Program.NonQuery("UPDATE tbl_user SET LastFriendUpdate = '" + user.LastFriendUpdate.ToString("yyyy-MM-dd HH:mm:ss") + "' WHERE PK_SteamID = '" + user.SteamId + "';");
            }
        }

        public static void DealWithGameNames(ListOfGames tempList)
        {
            var update = "";
            foreach (var gameHistory in tempList.List)
            {
                update += "INSERT IGNORE INTO tbl_games VALUES ('" + gameHistory.AppId + "','" + ChangeString(gameHistory.Name).Replace("\\", "\\\\")
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
        public static String ChangeString(String msg)
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
            public UInt64 SteamId { get; set; }

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