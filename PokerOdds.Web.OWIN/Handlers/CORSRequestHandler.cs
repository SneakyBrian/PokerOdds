using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Owin;
using PokerOdds.Web.OWIN.Interfaces;

namespace PokerOdds.Web.OWIN.Handlers
{
    public class CORSRequestHandler : IRequestHandler
    {
        public async Task HandleRequest(IOwinContext context)
        {
            context.Response.StatusCode = 200;
            context.Response.Headers["Access-Control-Allow-Origin"] = "*";
            context.Response.Headers["Access-Control-Allow-Methods"] = "GET";

            await context.Response.WriteAsync("");
        }

        public bool CanHandleRequest(IOwinContext context)
        {
            return context.Request.Method.ToUpperInvariant() == "OPTIONS";
        }
    }
}