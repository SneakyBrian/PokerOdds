using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Microsoft.Owin;
using Newtonsoft.Json;
using PokerOdds.HoldemOdds;
using PokerOdds.Web.OWIN.Interfaces;

namespace PokerOdds.Web.OWIN.Handlers
{
    public class HoldemOddsRequestHandler : IRequestHandler, IHasCache
    {
        private const int CACHE_DURATION_SECS = 5;

        private CacheItemPolicy _policy = new CacheItemPolicy { Priority = CacheItemPriority.Default };

        public async Task HandleRequest(IOwinContext context)
        {
            var pocket = context.Request.Query["pocket"];
            var board = context.Request.Query["board"];
            var callback = context.Request.Query["callback"];

            if (string.IsNullOrWhiteSpace(board))
            {
                board = string.Empty;
            }

            var odds = new TexasHoldemOdds { Pocket = HoldemOddsCalculator.SortCards(pocket), Board = HoldemOddsCalculator.SortCards(board) };

            var cacheOdds = Cache.Get(odds.GetCacheKey()) as TexasHoldemOdds;

            if (cacheOdds == null)
            {
                Cache.Add(odds.GetCacheKey(), odds, _policy);

                var calculator = new HoldemOddsCalculator();

                cacheOdds = await calculator.Calculate(odds, o => Cache.Set(odds.GetCacheKey(), odds, _policy));
            }
            //check If-None-Match header etag to see if it matches our data hash
            else if (context.Request.Headers["If-None-Match"] == cacheOdds.GetETag())
            {
                //if it does return 304 Not Modified
                context.Response.StatusCode = 304;
                return;
            }

            var jsonData = await Task.Factory.StartNew(() => JsonConvert.SerializeObject(cacheOdds));

            var result = string.Empty;

            if (string.IsNullOrWhiteSpace(callback))
            {
                context.Response.ContentType = "application/json";
                result = jsonData;
            }
            else
            {
                context.Response.ContentType = "application/javascript";
                result = callback + "(" + jsonData + ");";
            }

            //set the response as cached for cache duration
            context.Response.Headers["Cache-Control"] = string.Format("max-age={0}", CACHE_DURATION_SECS);

            //set etag
            context.Response.ETag = cacheOdds.GetETag();

            //Just in case, handle CORS
            context.Response.Headers["Access-Control-Allow-Origin"] = "*";

            await context.Response.WriteAsync(result);
        }

        public bool CanHandleRequest(IOwinContext context)
        {
            //get request, and we have pocket parameter
            return context.Request.Method.ToUpperInvariant() == "GET" && !string.IsNullOrWhiteSpace(context.Request.Query["pocket"]);
        }

        public ObjectCache Cache { get; set; }

 
    }
}