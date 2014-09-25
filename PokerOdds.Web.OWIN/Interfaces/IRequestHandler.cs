using System.Threading.Tasks;
using Microsoft.Owin;

namespace PokerOdds.Web.OWIN.Interfaces
{
    interface IRequestHandler
    {
        Task HandleRequest(IOwinContext context);

        bool CanHandleRequest(IOwinContext context);
    }
}
