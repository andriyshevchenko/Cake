using System.Threading.Tasks;

namespace Cakes.http
{
    public class TestEnd : IEndpoint
    { 
        public Task<IResponse> Act(IRequest request)
        {
            IResponse response = new TestRps();
            return Task.FromResult(response);
        }
    }

    public interface IEndpoint
    {
        Task<IResponse> Act(IRequest request);
    }
}
