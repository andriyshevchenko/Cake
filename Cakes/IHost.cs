namespace Cakes.http
{
    using System.Diagnostics;
    using System.Net.Sockets;
    using System.Threading.Tasks;

    /// <summary>
    /// Accepts incoming connection and processes a request.
    /// </summary>
    public interface IHost
    {
        Task Accept(SocketAsyncEventArgs socket);
    }
}
