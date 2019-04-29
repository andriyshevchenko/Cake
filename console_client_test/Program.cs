using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

// State object for receiving data from remote device.  
public class StateObject
{
    // Client socket.  
    public Socket workSocket = null;
    // Size of receive buffer.  
    public const int BufferSize = 256;
    // Receive buffer.  
    public byte[] buffer = new byte[BufferSize];
    // Received data string.  
    public StringBuilder sb = new StringBuilder();
}

public class AsynchronousClient
{
    // The port number for the remote device.  
    private const int port = 8888;

    private static void EndConnection(Socket client)
    {
        client.Shutdown(SocketShutdown.Both);
        client.Close();
        Console.WriteLine("\r\nClosed connection");
    }

    private static void StartClient()
    {
        IPHostEntry ipHostInfo = Dns.GetHostEntry("DESKTOP-D2G7SAG");
        IPAddress ipAddress = ipHostInfo.AddressList[2];
        IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

        List<Task> tasks = new List<Task>(100);

        for (int i = 0; i < 100; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                // Create a TCP/IP socket.  
                Socket client = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                client.BeginConnect(remoteEP,
                   new AsyncCallback(ConnectCallback), client);

                return Task.CompletedTask;
            }));
        }

        Task.WhenAll(tasks).Wait();
        Console.ReadLine();
    }

    private static void ConnectCallback(IAsyncResult ar)
    {
        // Retrieve the socket from the state object.  
        Socket client = (Socket)ar.AsyncState;

        // Complete the connection.  
        client.EndConnect(ar);

        Console.WriteLine("Socket connected to {0}",
            client.RemoteEndPoint.ToString());

        byte[] byteData = Encoding.UTF8.GetBytes("Message<EOF>");

        client.BeginSend(byteData, 0, byteData.Length, 0,
            new AsyncCallback(SendCallback), client
        );
    }

    private static void ReceiveCallback(IAsyncResult ar)
    {
        StateObject state = (StateObject)ar.AsyncState;
        Socket client = state.workSocket;

        try
        {
            // Read data from the remote device.  
            int bytesRead = client.EndReceive(ar);
            Console.WriteLine("Bytes read: " + bytesRead);

            if (bytesRead > 0)
            {
                // There might be more data, so store the data received so far.  
                state.sb.Append(Encoding.UTF8.GetString(state.buffer, 0, bytesRead));

                // Get the rest of the data.  
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            else
            {
                if (state.sb.Length > 1)
                {
                    Console.WriteLine(state.sb.ToString());
                    EndConnection(client);
                }
            }
        }
        catch (Exception e)
        {
            EndConnection(client);
            Console.WriteLine(e);
        }
    }

    private static void SendCallback(IAsyncResult ar)
    {
        Socket client = (Socket)ar.AsyncState;
        try
        {

            // Complete sending the data to the remote device.  
            int bytesSent = client.EndSend(ar);
            Console.WriteLine("Sent {0} bytes to server.", bytesSent);

            // Create the state object.  
            StateObject state = new StateObject
            {
                workSocket = client
            };

            // Begin receiving the data from the remote device.  
            client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReceiveCallback), state);

        }
        catch (Exception e)
        {
            EndConnection(client);
            Console.WriteLine(e);
        }
    }

    public static int Main(String[] args)
    {
        StartClient();
        return 0;
    }
}