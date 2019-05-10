namespace Cakes.http
{
    using System.Threading.Tasks;

    public interface ILogOutput
    {
        Task Write(params IText[] text);
    }
}
