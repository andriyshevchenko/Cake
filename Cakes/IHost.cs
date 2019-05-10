namespace Cakes.http
{
    using System.Threading.Tasks;

    /// <summary>
    /// Accepts incoming connection and processes a request.
    /// </summary>
    public interface IHost
    {
        /// <summary>
        /// Accept incoming network connection.
        /// </summary>
        /// <returns>A task that is completed when connection is succesfully accepted.</returns>
        Task Accept();
    }
}
