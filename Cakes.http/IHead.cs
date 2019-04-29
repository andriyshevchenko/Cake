namespace Cakes.http
{
    using System.Collections.Generic;

    public interface IHead
    {
        IEnumerable<string> Lines();
    }
}
