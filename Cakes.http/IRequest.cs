using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Cakes.http
{
    public class TestRq : IRequest
    {
        public IEnumerable<string> Lines()
        {   
            return Enumerable.Empty<string>();
        }

        public Stream Stream()
        {
            return new MemoryStream(new byte[0]);
        }
    }

    public interface IRequest : IHead, IBody
    {

    }
}
