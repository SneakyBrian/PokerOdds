using System;
using System.IO;
using System.Runtime.Caching;
using Microsoft.WindowsAzure.ServiceRuntime;
using Owin;
using PokerOdds.Web.OWIN.Routers;

namespace PokerOdds.Web.OWIN
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var cachePath = RoleEnvironment.IsAvailable ?
                RoleEnvironment.GetLocalResource("fileCache").RootPath :
                Path.Combine(Path.GetTempPath(), "PokerOdds.Web.OWIN");

            Directory.CreateDirectory(cachePath);

            var cache = new FileCache(cachePath);

            cache.MaxCacheSize = RoleEnvironment.IsAvailable ?
                RoleEnvironment.GetLocalResource("fileCache").MaximumSizeInMegabytes * 1024 :
                2048 * 1024; //2GB

            //when the max size is reached, purge everything more than 24 hours old and hope for the best!
            cache.MaxCacheSizeReached += (s, e) => cache.Flush(DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)));

            var router = new RequestRouter { Cache = cache };

            app.Run(router.HandleRequest);
        }
    }
}