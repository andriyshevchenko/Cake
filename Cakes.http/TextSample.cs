namespace Cakes.http
{
    using System.Net.Sockets;
    using System.Text;
    using System.Threading.Tasks;

    public class TextSample : IHub
    {
        public Task<byte[]> Act(byte[] message)
        {
            return Task.FromResult(Encoding.UTF8.GetBytes("A message from a server"));
        }
    }
}
