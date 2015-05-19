using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Steam_Data_Collection.Networking
{
    // State object for reading client data asynchronously
    public class StateObject
    {
        // Client  socket.
        // Size of receive buffer.
        public const int BufferSize = 1024;
        // Receive buffer.
        public byte[] Buffer = new byte[BufferSize];
        // Received data string.
        public byte[] Data = new byte[0];
        public Socket WorkSocket = null;
    }

    /// <summary>
    /// A custom built socket to handle any incoming data or connections
    /// </summary>
    internal class ServerSocket
    {
        public static ManualResetEvent allDone = new ManualResetEvent(false);

        public static void StartListening()
        {
            // Data buffer for incoming data.
            var bytes = new Byte[1024];

            // Create a TCP/IP socket.
            var listener = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for incoming connections.
            try
            {
                listener.Bind(new IPEndPoint(IPAddress.Any, 8220));
                listener.Listen(500);

                while (true)
                {
                    // Set the event to nonsignaled state.
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.
                    listener.BeginAccept(
                        AcceptCallback,
                        listener);

                    // Wait until a connection is made before continuing.
                    allDone.WaitOne();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.
            allDone.Set();

            // Get the socket that handles the client request.
            var listener = (Socket) ar.AsyncState;
            var handler = listener.EndAccept(ar);

            // Create the state object.
            var state = new StateObject();
            state.WorkSocket = handler;
            handler.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0,
                ReadCallback, state);
        }

        public static void ReadCallback(IAsyncResult ar)
        {
            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            var state = (StateObject) ar.AsyncState;
            var handler = state.WorkSocket;

            try
            {
                // Read data from the client socket. 
                SocketError se;
                var bytesRead = handler.EndReceive(ar, out se);

                if (bytesRead > 0)
                {
                    // There  might be more data, so store the data received so far.
                    var temp = new byte[state.Data.Length + bytesRead];
                    Array.Copy(state.Data, temp, state.Data.Length);
                    Array.Copy(state.Buffer, 0, temp, state.Data.Length, bytesRead);
                    state.Data = temp;

                    if (state.Data.Length != BitConverter.ToUInt16(state.Data, 0) && bytesRead != 0)
                    {
                        handler.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, ReadCallback, state);
                        return;
                    }

                    
                        PacketHandler.Handle(state.Data, state.WorkSocket);
                    
                    state.WorkSocket.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            state.WorkSocket.Close();
        }
    }
}