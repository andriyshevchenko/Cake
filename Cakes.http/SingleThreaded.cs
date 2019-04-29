namespace Cakes.http
{
    using System;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Single-threaded environment. The requests will be processed one-by-one.
    /// This class is not thread-safe and should be synchronized by the calling code.
    /// </summary>
    public class SingleThreaded : IHost
    {
        private ITextOutput output;
        private Socket transport;
        private IEndpoint endpoint;
        private TaskCompletionSource<bool> task;

        public SingleThreaded(Socket transport, IEndpoint endpoint) : this(transport, endpoint, new OpDebug())
        {

        }

        public SingleThreaded(Socket transport, IEndpoint endpoint, ITextOutput output)
        {
            this.transport = transport;
            this.endpoint = endpoint;
            this.output = output;
        }

        /// <summary>
        /// Accepts a connection and processes request. 
        /// This is NOT thread-safe already. WIP
        /// </summary>
        /// <param name="socketOp"></param>
        /// <returns>A task representing an operation.</returns>
        public Task Accept(SocketAsyncEventArgs socketOp)
        {
            output.Write(
                new Str("SingleThreaded.Accept, thread: "),
                new ThreadId(),
                new Str(" Socket operation: "),
                new ToStr(socketOp.LastOperation)
            );

            if (task != null && task.Task != null && task.Task.IsCompletedSuccessfully)
            {
                throw new ConcurrencyException("Concurrent calls to SingleThreaded.Accept are impossible");
            }
            this.task = new TaskCompletionSource<bool>(false);

            try
            {
                socketOp.Completed += SocketOp_Completed;
                transport.AcceptAsync(socketOp);
            }
            catch (System.Exception exc)
            {
                output.Write(new ToStr(exc)).Wait();
                socketOp.Completed -= SocketOp_Completed;
                task.TrySetException(exc);
            }
            return task.Task ?? Task.CompletedTask;
        }

        private void SocketOp_Completed(object sender, SocketAsyncEventArgs e)
        {
            SocketAsyncOperation lastOperation = e.LastOperation;

            switch (lastOperation)
            {
                case SocketAsyncOperation.Accept:
                    EndAccept(e);
                    break;
                case SocketAsyncOperation.Connect:
                    break;
                case SocketAsyncOperation.Disconnect:
                    break;
                case SocketAsyncOperation.None:
                    break;
                case SocketAsyncOperation.Receive:
                    EndReceive(e);
                    break;
                case SocketAsyncOperation.ReceiveFrom:
                    break;
                case SocketAsyncOperation.ReceiveMessageFrom:
                    break;
                case SocketAsyncOperation.Send:
                    EndSend(e);
                    break;
                case SocketAsyncOperation.SendPackets:
                    break;
                case SocketAsyncOperation.SendTo:
                    break;
                default:
                    break;
            }
            output.Write(
                new Format(
                    "SingleThreaded.SocketOp_Completed, thread: {0}, operation: {1}",
                    new ThreadId(),
                    new ToStr(lastOperation)
                )
            );
        }

        private async void EndReceive(SocketAsyncEventArgs e)
        {
            try
            {
                output.Write(new StrBr("Receiving...")).Wait();
                IResponse response = await endpoint.Act(new TestRq()).ConfigureAwait(false);
                byte[] bytes = Encoding.UTF8.GetBytes("A message from a server");
                e.SetBuffer(bytes, e.Offset, bytes.Length);
                if (!transport.SendAsync(e))
                {
                    EndSend(e);
                };
            }
            catch (Exception exc)
            {
                output.Write(new ToStr(exc)).Wait();
                if (e.SocketError != SocketError.Success)
                {
                    output.Write(new StrBr("Socket error: " + e.SocketError)).Wait();
                }
                e.Completed -= SocketOp_Completed;
                task.SetException(exc);
                task = null;
            }
        }

        private void EndSend(SocketAsyncEventArgs e)
        {
            output.Write(new StrBr("Sent."));
            e.Completed -= SocketOp_Completed;
            task.TrySetResult(true);
            task = null;
        }

        private void EndAccept(SocketAsyncEventArgs e)
        {
            try
            {
                output.Write(new StrBr("Accepted connection from")).Wait();
                //refine SocketAsyncEventArgs before receive
                e.SetBuffer(0, 16384);
                output.Write(new StrBr("Set buffer")).Wait();
                if (!transport.ReceiveAsync(e))
                {
                    EndReceive(e);
                }
                output.Write(new StrBr("Start receiving")).Wait();
            }
            catch (System.Exception exc)
            {
                output.Write(new ToStr(exc)).Wait();
                e.Completed -= SocketOp_Completed;
                task.TrySetException(exc);
                task = null;
                //Close connection;  
            }
        }
    }
}
