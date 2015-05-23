using System;
using System.Collections.Generic;
using Steam_Data_Collection_Client.Networking.Packets;
using Steam_Data_Collection_Client.Objects;

namespace Steam_Data_Collection
{
    internal static class GetInformation
    {
        /// <summary>
        /// Returns the general statistics about the server
        /// </summary>
        /// <param name="hostId">The host id of the host that requested the info</param>
        /// <returns>A string containing the general statistics of the server</returns>
        public static String ShowGenStats(ushort hostId)
        {
            var users = "Number of users: " + Program.Count("SELECT COUNT(PK_SteamId) FROM tbl_user;");
            var friend = "Number of seperate friend relations: " +
                         Program.Count("SELECT COUNT(PK_FriendId) FROM tbl_frienddate;");
            var gcollectionlink = "Number of library screenings: " +
                                  Program.Count("SELECT COUNT(*) FROM tbl_gcollectionlink;");
            var gcollection = "Number of game screenings: " +
                              Program.Count("SELECT COUNT(*) FROM tbl_gcollection;");
            var games = "Number of games collected: " + Program.Count("SELECT COUNT(*) FROM tbl_games;");

            return users + "\n" + friend + "\n" + gcollectionlink + "\n" + gcollection + "\n" + games + "\n";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static List<User> ShowPlayerStats(StdData user)
        {
            var sid = user.Text;
            var dt =
                Program.Select(
                    "SELECT tbl_gcollection.PK_GameHistory, tbl_User.PK_SteamID, UserName, CustomURL, VisibilityState, MemberSince, Location, RealName, LastLogOff, LastSummaryUpdate, LastFriendUpdate, LastGameUpdate, Timestamp, FK_AppId, tbl_games.Name, MinsOnRecord, MinsLast2Weeks FROM tbl_user LEFT JOIN (SELECT tbl_gcollectionlink.PK_SteamID, Timestamp FROM tbl_gcollectionlink RIGHT JOIN tbl_user ON tbl_gcollectionlink.PK_SteamId = tbl_user.PK_SteamID WHERE tbl_user.PK_SteamId = '" +
                    sid + "' OR UserName = '" + sid +
                    "' GROUP BY timestamp ORDER BY timestamp asc) t ON tbl_user.PK_SteamID = t.PK_SteamID LEFT JOIN tbl_gcollection ON t.PK_SteamID = tbl_gcollection.FK_SteamID LEFT JOIN tbl_games ON tbl_gcollection.FK_AppID = tbl_games.PK_AppID WHERE tbl_User.PK_SteamID = '" +
                    sid + "' OR tbl_user.UserName = '" + sid + "';");

            var count = 0;
            var list = new List<User> {new User()};
            while (count != dt.Rows.Count)
            {
                if (list[list.Count - 1].SteamId != (ulong) dt.Rows[count][1])
                {
                    if (list[list.Count - 1].SteamId == 0)
                    {
                        list[list.Count - 1].SteamId = (ulong) dt.Rows[count][1];
                    }
                    else
                    {
                        list.Add(new User());
                        list[list.Count - 1].SteamId = (ulong) dt.Rows[count][1];
                    }
                    list[list.Count - 1].UserName = (string) dt.Rows[count][2];
                    list[list.Count - 1].CustomUrl = (string) dt.Rows[count][3];
                    list[list.Count - 1].VisibilityState = (bool) dt.Rows[count][4];
                    list[list.Count - 1].LastSummaryUpdate = (DateTime) dt.Rows[count][9];
                    list[list.Count - 1].LastLogOff = (DateTime) dt.Rows[count][8];
                    if (list[list.Count - 1].VisibilityState)
                    {
                        list[list.Count - 1].Location = (string) dt.Rows[count][6];
                        list[list.Count - 1].RealName = (string) dt.Rows[count][7];
                        list[list.Count - 1].MemberSince = (DateTime) dt.Rows[count][5];
                        list[list.Count - 1].LastGameUpdate = (DateTime) dt.Rows[count][11];
                        if (dt.Rows[count][10].GetType() != typeof (DBNull))
                        {
                            list[list.Count - 1].LastFriendUpdate = (DateTime) dt.Rows[count][10];
                        }
                    }
                }

                if (list[list.Count - 1].VisibilityState)
                {
                    var temp = new GameHistory
                    {
                        AppId = (int) dt.Rows[count][13],
                        OnRecord = (int) dt.Rows[count][15],
                        Last2Weeks = (int) dt.Rows[count][16]
                    };
                    if (dt.Rows[count][14].GetType() != typeof (DBNull))
                    {
                        temp.Name = (string) dt.Rows[count][14];
                    }
                    list[list.Count - 1].ListOfGames.Add(temp);
                }
                count++;
            }

            if (list[0].SteamId == 0)
            {
                list.RemoveAt(0);
            }

            return list;
        }
    }
}