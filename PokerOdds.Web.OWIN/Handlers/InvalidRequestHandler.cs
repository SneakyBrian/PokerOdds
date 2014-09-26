using System.Threading.Tasks;
using Microsoft.Owin;
using PokerOdds.Web.OWIN.Interfaces;

namespace PokerOdds.Web.OWIN.Handlers
{
    public class InvalidRequestHandler : IRequestHandler
    {
        public async Task HandleRequest(IOwinContext context)
        {
            context.Response.StatusCode = 400;
            context.Response.ContentType = "text/plain";
            await context.Response.WriteAsync("Bad Request");
        }


        public bool CanHandleRequest(IOwinContext context)
        {
            return true;
        }
    }
}