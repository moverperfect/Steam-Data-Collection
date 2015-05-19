using System;

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
            var gcollectionlink = "Number of seperate full game collections: " +
                                  Program.Count("SELECT COUNT(*) FROM tbl_gcollectionlink;");
            var gcollection = "Number of user/games collected: " +
                              Program.Count("SELECT COUNT(*) FROM tbl_gcollection;");
            var games = "Number of games collected: " + Program.Count("SELECT COUNT(*) FROM tbl_games;");

            return users + "\n" + friend + "\n" + gcollectionlink + "\n" + gcollection + "\n" + games + "\n";
        }
    }
}