namespace Cakes.http
{
    using System.IO;

    public interface IBody
    {
        Stream Stream();
    }
}
