namespace Cakes.http
{
    using System.Threading.Tasks;

    public class OpConsole : ITextOutput
    {
        public async Task Write(params IText[] text)
        {
            for (int i = 0; i < text.Length; i++)
            {
                System.Console.Write(await text[i].String().ConfigureAwait(false));
            }

            System.Console.WriteLine(); 
        }
    }
}
