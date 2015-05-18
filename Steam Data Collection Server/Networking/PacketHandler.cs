using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Sockets;
using Steam_Data_Collection_Client.Networking.Packets;
using Steam_Data_Collection_Client.Objects;

namespace Steam_Data_Collection.Networking
{
    /// <summary>
    /// Handles all incoming packets into the server after fully recieving them
    /// </summary>
    internal static class PacketHandler
    {
        /// <summary>
        /// Handles all incoming packets into the server after fully recieving them
        /// </summary>
        /// <param name="packet">The packet that was recieved</param>
        /// <param name="clientSocket">The socket, used for sending back data to the client</param>
        public static void Handle(byte[] packet, Socket clientSocket)
        {
            // Get the packet length and type
            var packetLength = BitConverter.ToUInt16(packet, 0);
            var packetType = BitConverter.ToUInt16(packet, 2);

            Console.WriteLine("Recieved packet of length: {0} and Type: {1}", packetLength, packetType);

            SqlConnecter connecter;

            // Packet types:
            // 2000 - Update the steam token
            // 2001 - Update the host id
            // 2002 - Request to Update all the things
            // 2003 - Request to Update the summary
            // 3003 - Information to update the summary to the server
            switch (packetType)
            {
                case 2000:
                    clientSocket.Send(new StdData(Program.SteamToken, 0, 0, 2000).Data);
                    break;

                case 2001:
                    clientSocket.Send(new StdData("21", 0, 0, 2001).Data);
                    break;

                case 2002:
                    clientSocket.Send(DataDealer.UpdateAll(new StdData(packet).MachineId));
                    break;

                case 2003:
                    clientSocket.Send(DataDealer.UpdateSum(true, new StdData(packet).MachineId).Data);
                    break;

                case 3003:
                    var list = new ListOfUsers(packet);
                    try
                    {
                        DataDealer.DealWithSum(list);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    finally
                    {
                        clientSocket.Send(new StdData("", 0, 0, 1000).Data);
                    }
                    break;
            }
            clientSocket.Close();
        }
    }
}