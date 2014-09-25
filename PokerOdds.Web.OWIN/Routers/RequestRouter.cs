using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Owin;
using PokerOdds.Web.OWIN.Handlers;
using PokerOdds.Web.OWIN.Interfaces;

namespace PokerOdds.Web.OWIN.Routers
{
    public class RequestRouter : IRequestHandler, IHasCache
    {
        public async Task HandleRequest(IOwinContext context)
        {
            var requestHandler = new HoldemOddsRequestHandler { Cache = Cache };
            if (requestHandler.CanHandleRequest(context))
            {
                await requestHandler.HandleRequest(context);
                return;
            }

            var invalidRequestHandler = new InvalidRequestHandler();
            if (invalidRequestHandler.CanHandleRequest(context))
            {
                await invalidRequestHandler.HandleRequest(context);
                return;
            } 
        }

        public bool CanHandleRequest(IOwinContext context)
        {
            return true;
        }

        public ObjectCache Cache { get; set; }

    }
}