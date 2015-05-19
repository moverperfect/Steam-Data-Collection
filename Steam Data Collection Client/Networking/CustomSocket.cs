using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Steam_Data_Collection_Client.Networking
{
    /// <summary>
    /// A custom built socket to handle any incoming data or connections
    /// </summary>
    internal static class CustomSocket
    {
        public static byte[] StartClient(byte[] data)
        {
            while (true)
            {
                // Data buffer for incoming data.
                var bytes = new byte[1024];

                // Connect to a remote device.
                try
                {
                    var remoteEp = new IPEndPoint(Program.IpAddress, Program.Port);

                    // Create a TCP/IP  socket.
                    var sender = new Socket(AddressFamily.InterNetwork,
                        SocketType.Stream, ProtocolType.Tcp);

                    // Connect the socket to the remote endpoint. Catch any errors.
                    try
                    {
                        sender.Connect(remoteEp);

                        Console.WriteLine("Socket connected to {0}",
                            sender.RemoteEndPoint);

                        // Encode the data string into a byte array.
                        var msg = data;

                        // Send the data through the socket.
                        var bytesSent = sender.Send(msg);

                        // Receive the response from the remote device.
                        var bytesRec = sender.Receive(bytes);

                        // Release the socket.
                        sender.Shutdown(SocketShutdown.Both);
                        sender.Close();

                        Array.Resize(ref bytes, bytesRec);

                        if (bytesRec < 8)
                        {
                            throw new Exception();
                        }

                        return bytes;
                    }
                    catch (ArgumentNullException ane)
                    {
                        Console.WriteLine("ArgumentNullException : {0}", ane.Message);
                    }
                    catch (SocketException se)
                    {
                        Console.WriteLine("SocketException : {0}", se.Message);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Unexpected exception : {0}", e.Message);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                Thread.Sleep(500);
            }
        }
    }
}