namespace Cakes.http
{
    using System.Threading.Tasks;

    public class ThreadId : IText
    {
        public Task<string> String()
        {
            return Task.FromResult(System.Threading.Thread.CurrentThread.ManagedThreadId.ToString());
        }
    }
}
