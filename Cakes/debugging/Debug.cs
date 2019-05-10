namespace Cakes.http
{
    using System.Diagnostics;
    using System.Threading.Tasks;

    public class OpDebug : ILogOutput
    { 
        public async Task Write(params IText[] text)
        {
            for (int i = 0; i < text.Length; i++)
            {
                Debug.Write(await text[i].String().ConfigureAwait(false));
            }
            Debug.Write("\r\n"); 
        }
    }
}
