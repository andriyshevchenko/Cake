namespace Cakes
{
    using System.Threading.Tasks;
    
    public interface IHub
    {
        Task<byte[]> Act(byte[] message);
    }
}
