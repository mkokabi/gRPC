using static Server.Calc;
using System.Threading.Tasks;
using Grpc.Core;

namespace Server
{

    public class CalcService : CalcBase {
        public async override Task<AddResponse> Add(AddRequest request, ServerCallContext context) 
        {
            return await Task.FromResult(new AddResponse
            {
                C = request.A + request.B
            });
        }
    }
}