namespace Cakes.http
{
    using System.Threading.Tasks;

    public interface IText
    {
        Task<string> String();
    }
}
