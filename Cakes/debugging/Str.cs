namespace Cakes.http
{
    using System.Threading.Tasks;

    public class Str : IText
    {
        private string item;

        public Str(string item)
        {
            this.item = item;
        }

        public Task<string> String()
        {
            return Task.FromResult(item);
        }
    }
}
