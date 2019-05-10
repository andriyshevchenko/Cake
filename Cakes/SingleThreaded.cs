namespace Cakes.http
{
    using System;
    using System.Collections.Generic;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// This class creates a single large buffer which can be divided up and assigned to SocketAsyncEventArgs objects for use
    /// with each socket I/O operation.  This enables bufffers to be easily reused and gaurds against fragmenting heap memory.
    /// 
    /// The operations exposed on the BufferManager class are not thread safe.
    /// </summary>
    class SocketMemory
    {
        int m_numBytes;                 // the total number of bytes controlled by the buffer pool
        byte[] m_buffer;                // the underlying byte array maintained by the Buffer Manager
        Stack<int> m_freeIndexPool;     // 
        int m_currentIndex;
        int m_bufferSize;

        public SocketMemory(int totalBytes, int bufferSize)
        {
            m_numBytes = totalBytes;
            m_currentIndex = 0;
            m_bufferSize = bufferSize;
            m_freeIndexPool = new Stack<int>();
        }

        /// <summary>
        /// Allocates buffer space used by the buffer pool
        /// </summary>
        public void InitBuffer()
        {
            // create one big large buffer and divide that out to each SocketAsyncEventArg object
            m_buffer = new byte[m_numBytes];
        }

        /// <summary>
        /// Assigns a buffer from the buffer pool to the specified SocketAsyncEventArgs object
        /// </summary>
        /// <returns>true if the buffer was successfully set, else false</returns>
        public bool SetBuffer(SocketAsyncEventArgs args)
        {

            if (m_freeIndexPool.Count > 0)
            {
                args.SetBuffer(m_buffer, m_freeIndexPool.Pop(), m_bufferSize);
            }
            else
            {
                if ((m_numBytes - m_bufferSize) < m_currentIndex)
                {
                    return false;
                }
                args.SetBuffer(m_buffer, m_currentIndex, m_bufferSize);
                m_currentIndex += m_bufferSize;
            }
            return true;
        }

        /// <summary>
        /// Removes the buffer from a SocketAsyncEventArg object.  This frees the buffer back to the 
        /// buffer pool
        /// </summary>
        public void FreeBuffer(SocketAsyncEventArgs args)
        {
            m_freeIndexPool.Push(args.Offset);
            args.SetBuffer(null, 0, 0);
        }
    }

    /// <summary>
    /// This class is designed for use as the object to be assigned to the SocketAsyncEventArgs.UserToken property. 
    /// </summary>
    class AsyncUserToken
    {
        public AsyncUserToken() : this(null) { }

        public AsyncUserToken(Socket socket)
        {
            Socket = socket;
        }

        public Socket Socket { get; set; }
    }

    /// <summary>
    /// Single-threaded environment. The requests will be processed one-by-one.
    /// This class is not thread-safe and should be synchronized by the calling code.
    /// </summary>
    public class SingleThreaded : IHost
    {
        private ILogOutput output;
        private Socket transport;
        private IHub hub;
        private TaskCompletionSource<bool> task;
        private SocketAsyncEventArgsPool socketOp = new SocketAsyncEventArgsPool(64);

        public SingleThreaded(Socket transport, IHub hub) : this(transport, hub, new OpDebug())
        {

        }

        public SingleThreaded(Socket transport, IHub hub, ILogOutput output)
        {
            this.transport = transport;
            this.hub = hub;
            this.output = output;
        }

        /// <summary>
        /// Accepts a connection and processes request. 
        /// This is NOT thread-safe already. WIP
        /// </summary>
        /// <returns>A task that is completed when connection is succesfully accepted.</returns>
        public Task Accept()
        {
            output.Write(
                new Str("SingleThreaded.Accept, thread: "),
                new ThreadId()
            );

            if (task != null && task.Task != null && task.Task.IsCompletedSuccessfully)
            {
                throw new ConcurrencyException("Concurrent calls to SingleThreaded.Accept are impossible");
            }
            this.task = new TaskCompletionSource<bool>(false);
            SocketAsyncEventArgs socketOp = new SocketAsyncEventArgs();
            try
            {
                socketOp.Completed += SocketOp_Completed;
                socketOp.UserToken = new AsyncUserToken();
                socketOp.AcceptSocket = null;
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
                byte[] bytes = await hub.Act(e.Buffer).ConfigureAwait(false);
                AsyncUserToken token = (AsyncUserToken)e.UserToken;
                if (e.SocketError != SocketError.Success)
                {
                    //process error.
                }
                if (e.BytesTransferred > 0)
                {
                    if (!token.Socket.SendAsync(e))
                    {
                        EndSend(e);
                    };
                }
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
            if (e.SocketError == SocketError.Success)
            {
                output.Write(new StrBr("Sent."));
                AsyncUserToken token = (AsyncUserToken)e.UserToken;
                token.Socket.Shutdown(SocketShutdown.Send);
                // read the next block of data send from the client
                e.Completed -= SocketOp_Completed;
            }
            task.TrySetResult(true);
            task = null;
        }

        private void EndAccept(SocketAsyncEventArgs e)
        {
            try
            {
                output.Write(new StrBr("Accepted connection from")).Wait();
                output.Write(new StrBr("Set buffer")).Wait();
                SocketAsyncEventArgs readEventArgs = new SocketAsyncEventArgs
                {
                    UserToken = new AsyncUserToken()
                }; 
                readEventArgs.SetBuffer(new byte[16384], 0, 16384);
                ((AsyncUserToken)readEventArgs.UserToken).Socket = e.AcceptSocket;
                if (!e.AcceptSocket.ReceiveAsync(readEventArgs))
                {
                    EndReceive(readEventArgs);
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
