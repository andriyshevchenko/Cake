namespace Cakes.http
{
    using System;
    using System.Threading.Tasks;

    public class OpConsole : ILogOutput
    {
        public async Task Write(params IText[] text)
        {
            for (int i = 0; i < text.Length; i++)
            {
                Console.Write(await text[i].String().ConfigureAwait(false));
            }

            Console.WriteLine(); 
        }
    }
}
