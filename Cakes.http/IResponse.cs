namespace Cakes.http
{
    public class TestRps : TestRq, IResponse
    {

    }

    public interface IResponse : IHead, IBody
    {

    }
}
