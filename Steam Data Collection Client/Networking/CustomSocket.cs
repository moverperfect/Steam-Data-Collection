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
                var buffer = new byte[4];

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
                        sender.Send(msg);

                        sender.ReceiveTimeout = 5000;
                        sender.ReceiveBufferSize = 4;
                        sender.Receive(buffer, 4, SocketFlags.None);

                        sender.ReceiveBufferSize = (int) (BitConverter.ToUInt32(buffer, 0) - 4);

                        Array.Resize(ref buffer, (int)BitConverter.ToUInt32(buffer, 0));
                        var bytesrec = sender.Receive(buffer, 4, buffer.Length - 4, SocketFlags.None);

                        var totalBytesRec = bytesrec + 4;

                        while (totalBytesRec != buffer.Length)
                        {
                            totalBytesRec += sender.Receive(buffer, totalBytesRec, buffer.Length - totalBytesRec,
                                SocketFlags.None);
                        }

                        // Receive the response from the remote device.
                        //bytesRec = sender.Receive(buffer);

                        // Release the socket.
                        sender.Shutdown(SocketShutdown.Both);
                        sender.Close();

                        if (buffer.Length < 8)
                        {
                            throw new Exception();
                        }

                        return buffer;
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