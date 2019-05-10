namespace Cakes.http
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Net.Sockets;

    public sealed class SocketAsyncEventArgsPool
    {
        ConcurrentQueue<SocketAsyncEventArgs> queue;

        public SocketAsyncEventArgsPool(int capacity)
        {
            this.queue = new ConcurrentQueue<SocketAsyncEventArgs>();
        }

        public SocketAsyncEventArgs Pop()
        {
            if (this.queue.TryDequeue(out SocketAsyncEventArgs args))
            {
                return args;
            }
            return null;
        }
        public void Push(SocketAsyncEventArgs item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("Items added to a SocketAsyncEventArgsPool cannot be null");
            }
            this.queue.Enqueue(item);
        }
    }
}
