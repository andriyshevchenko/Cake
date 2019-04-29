using Cakes.http;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace console_test
{
    class Program
    {
        public static ManualResetEvent waithandle = new ManualResetEvent(false);

        async static Task Main(string[] args)
        {
            IPHostEntry ipHostEntry = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in ipHostEntry.AddressList)
            {
                Console.WriteLine(ip);
            }
            IPAddress ipAddress = ipHostEntry.AddressList[2];
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 8888);

            // Create a TCP/IP socket.  
            Socket listener = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);
            // Bind the socket to the local endpoint and listen for incoming connections.  
            try
            {
                listener.Bind(ipEndPoint);
                listener.Listen(10000);

                IHost host = new SingleThreaded(listener, new TextSample(), new OpConsole());

                while (true)
                {
                    waithandle.Reset();
                    SocketAsyncEventArgs result = new SocketAsyncEventArgs();

                    await host.Accept(result)
                        .ContinueWith(task =>
                            {
                                waithandle.Set();
                            },
                            TaskContinuationOptions.OnlyOnRanToCompletion
                        );

                    waithandle.WaitOne();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.ReadLine();
        }

        // State object for reading client data asynchronously  
        public class StateObject
        {
            // Client  socket.  
            public Socket workSocket = null;
            // Size of receive buffer.  
            public const int BufferSize = 1024;
            // Receive buffer.  
            public byte[] buffer = new byte[BufferSize];
            // Received data string.  
            public StringBuilder sb = new StringBuilder();
        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                Console.WriteLine("Method: AcceptCallback, thread: " + Thread.CurrentThread.ManagedThreadId);
                // Signal the main thread to continue.  
                waithandle.Set();

                // Get the socket that handles the client request.  
                Socket listener = (Socket)ar.AsyncState;
                Socket handler = listener.EndAccept(ar);

                // Create the state object.  
                StateObject state = new StateObject
                {
                    workSocket = handler
                };
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static void ReadCallback(IAsyncResult ar)
        {
            try
            {
                Console.WriteLine("Method: ReadCallback, thread: " + Thread.CurrentThread.ManagedThreadId);
                String content = String.Empty;

                // Retrieve the state object and the handler socket  
                // from the asynchronous state object.  
                StateObject state = (StateObject)ar.AsyncState;
                Socket handler = state.workSocket;

                // Read data from the client socket.   
                int bytesRead = handler.EndReceive(ar);
                Console.WriteLine("Bytes read: " + bytesRead);
                if (bytesRead > 0)
                {
                    // There  might be more data, so store the data received so far.  
                    state.sb.Append(Encoding.UTF8.GetString(
                        state.buffer, 0, bytesRead));

                    Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
                            content.Length, content);
                    // Echo the data back to the client.  
                    Console.WriteLine("Method: Send, thread: " + Thread.CurrentThread.ManagedThreadId);

                    ThreadPool.UnsafeQueueUserWorkItem(_state =>
                        {
                            Console.WriteLine("Queued work on a thread pool: " + Thread.CurrentThread.ManagedThreadId);

                            Socket socket = (Socket)_state;
                            // Convert the string data to byte data using UTF8 encoding.  
                            byte[] byteData = Encoding.UTF8.GetBytes("A message from a server " + socket.GetHashCode());
                            // Begin sending the data to the remote device.  
                            handler.BeginSend(byteData, 0, byteData.Length, 0,
                                new AsyncCallback(SendCallback), handler);
                        }, handler
                    );
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void SendCallback(IAsyncResult ar)
        {
            Console.WriteLine("Method: SendCallback, thread: " + Thread.CurrentThread.ManagedThreadId);

            try
            {
                // Retrieve the socket from the state object.  
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}