using System;
using System.Data;
using System.Net.Sockets;

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
            switch (packetType)
            {
                
            }
            clientSocket.Close();
        }
    }
}