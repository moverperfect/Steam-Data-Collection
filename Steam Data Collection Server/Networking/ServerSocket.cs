using System;
using System.Net.Sockets;
using Steam_Data_Collection_Client.Networking;

namespace Steam_Data_Collection.Networking
{
    /// <summary>
    /// The child socket for communicating with a client/server, specialised for server application
    /// </summary>
    internal class ServerSocket : CustomSocket
    {
        /// <summary>
        /// Overides the function inside the base class, forwards the packet to the server PacketHandler
        /// </summary>
        /// <param name="packet">The packet that has been recieved</param>
        /// <param name="clientSocket">The socket that the data has been recieved from</param>
        public override void HandlePacket(Byte[] packet, Socket clientSocket)
        {
            PacketHandler.Handle(packet, clientSocket);
        }
    }
}