using System;
using System.Collections.Generic;
using Steam_Data_Collection_Client.Networking.Packets;

namespace Steam_Data_Collection
{
    internal static class DataDealer
    {
        /// <summary>
        /// A list of the current ids being scanned on the summary
        /// </summary>
        public static List<CurrentScan> CurrSumList = new List<CurrentScan>();

        /// <summary>
        /// A list of the current ids being scanned on the game
        /// </summary>
        public static List<CurrentScan> CurrGameList = new List<CurrentScan>();

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
            return new ListOfId(new List<ulong>(), 0, 0, 2000).Data;
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


            return new ListOfId(listOfIds, 0, 0, 2003);
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
                foreach (var t in CurrGameList)
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

            return new ListOfId(listOfIds, 0, 0, 2004);
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
                             user.UserName.Replace("'", "\\\'") + "', LastLogOff = '" +
                             user.LastLogOff.ToString("yyyy-MM-dd HH:mm:ss") + "', CustomURL = '" + user.CustomUrl +
                             "', LastSummaryUpdate = '" + user.LastSummaryUpdate.ToString("yyyy-MM-dd HH:mm:ss") + "'";

                if (user.VisibilityState)
                {
                    if (user.RealName != null)
                    {
                        update += ", RealName = '" + user.RealName.Replace("'", "\\\'") + "'";
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
                    "INSERT INTO tbl_gcollection(`FK_SteamID`,`FK_TimeStamp`,`FK_AppID`,`MinsLast2Weeks`,`MinsOnRecord`) VALUES ";
                foreach (var game in user.ListOfGames)
                {
                    insertGCollection += "('" + user.SteamId + "', '" +
                                         user.LastGameUpdate.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + game.AppId +
                                         "', '" + game.Last2Weeks + "', '" +
                                         game.OnRecord +
                                         "'),";
                }

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
        /// Holds the information about users we are currently scanning
        /// </summary>
        public struct CurrentScan
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