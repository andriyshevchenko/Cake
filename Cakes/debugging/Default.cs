namespace Cakes.http
{
    using System.Threading.Tasks;

    public class Default : ILogOutput
    {
        public Task Write(params IText[] text)
        {
            return Task.CompletedTask;
        }
    }
}
