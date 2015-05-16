using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Sockets;
using Steam_Data_Collection_Client.Networking.Packets;

namespace Steam_Data_Collection
{
    static class DataDealer
    {
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

        /// <summary>
        /// A list of the current ids being scanned on the summary
        /// </summary>
        public static List<CurrentScan> CurrSumList = new List<CurrentScan>();

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

            return null;
        }

        /// <summary>
        /// Return a list of ids that need to be updated in the order they need to be updated
        /// </summary>
        public static ListOfId UpdateSum(bool mark, int hostId)
        {
            var dt = (DataTable)Program.SqlDb.Select("SELECT PK_SteamId FROM tbl_user WHERE LastSummaryUpdate < NOW() - Interval " + Program.UpdateInterval + "  OR LastSummaryUpdate is Null ORDER BY LastSummaryUpdate;");
            var listOfIds = new List<UInt64>();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                listOfIds.Add((UInt64)dt.Rows[i][0]);
            }

            if (mark)
            {
                foreach (CurrentScan t in CurrSumList)
                {
                    listOfIds.Remove(t.SteamId);
                }
                foreach (var id in listOfIds)
                {
                    CurrSumList.Add(new CurrentScan() {HostId = hostId, SteamId = id, TimeOfScan = DateTime.Now});
                }
            }

            var l = 100;

            if (listOfIds.Count < 100)
            {
                l = listOfIds.Count;
            }

            listOfIds = listOfIds.GetRange(0,l);

            return new ListOfId(listOfIds, 0, 0, 2002);
        }
    }
}
