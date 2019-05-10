namespace Cakes.http
{
    using System.Threading.Tasks;

    /// <summary>
    /// Appends "\r\n" to end of the string.
    /// </summary>
    public class StrBr : IText
    {
        private string item;

        public StrBr(string item)
        {
            this.item = item;
        }

        public Task<string> String()
        {
            return Task.FromResult(item+"\r\n");
        }
    }
}
