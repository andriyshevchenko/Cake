namespace Cakes.http
{
    using System;
    using System.Threading.Tasks;

    public class ToStr : IText
    {
        private Object item;

        public ToStr(Object item)
        {
            this.item = item;
        }

        public Task<string> String()
        {
            return Task.FromResult(item.ToString());
        }
    }
}
