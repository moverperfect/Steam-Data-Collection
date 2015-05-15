using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Steam_Data_Collection_Client.Networking.Packets;

namespace Steam_Data_Collection
{
    static class DataDealer
    {
        public static void UpdateSum(Socket socket)
        {
            var dt = (DataTable)Program.SqlDb.Select("SELECT PK_SteamId FROM tbl_user WHERE LastSummaryUpdate < NOW() - Interval 1 week  OR LastSummaryUpdate is Null ORDER BY LastSummaryUpdate;");
            var listOfIds = new List<UInt64>();
            var l = 100;
            if (dt.Rows.Count < 100)
            {
                l = dt.Rows.Count;
            }
            for (int i = 0; i < l; i++)
            {
                listOfIds.Add((UInt64)dt.Rows[i][0]);
            }

            var packet = new ListOfId(listOfIds, 0, 0, 2002);
            socket.Send(packet.Data);
        }
    }
}
