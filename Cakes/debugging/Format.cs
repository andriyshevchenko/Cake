namespace Cakes.http
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class Format : IText
    {
        private string formatted;
        private IText[] parameters;

        public Format(string formatted, params IText[] parameters)
        {
            this.formatted = formatted;
            this.parameters = parameters;
        }

        public Task<string> String()
        {
            List<object> list = new List<object>(parameters.Length);
            for (int i = 0; i < parameters.Length; i++)
            {
                list.Add(parameters[i]);
            }
            return Task.FromResult(string.Format(formatted, list));
        }
    }
}
