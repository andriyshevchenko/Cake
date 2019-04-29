namespace Cakes.http
{
    using System.Threading.Tasks;

    public class Default : ITextOutput
    {
        public Task Write(params IText[] text)
        {
            return Task.CompletedTask;
        }
    }

    public interface ITextOutput
    {
        Task Write(params IText[] text);
    }
}
