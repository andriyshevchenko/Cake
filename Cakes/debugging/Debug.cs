namespace Cakes.http
{
    using System.Threading.Tasks;

    public class OpDebug : ITextOutput
    { 
        public async Task Write(params IText[] text)
        {
            for (int i = 0; i < text.Length; i++)
            {
                System.Diagnostics.Debug.Write(await text[i].String().ConfigureAwait(false));
            }
            System.Diagnostics.Debug.Write("\r\n"); 
        }
    }
}
