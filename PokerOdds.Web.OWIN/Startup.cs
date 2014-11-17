using System;
using System.IO;
using System.Runtime.Caching;
using Microsoft.WindowsAzure.ServiceRuntime;
using Owin;
using PokerOdds.Web.OWIN.Routers;
using System.Linq;
using PokerOdds.HoldemOdds;
using Ionic.Zip;
using Newtonsoft.Json;
using System.Reflection;
using System.Threading.Tasks;

namespace PokerOdds.Web.OWIN
{
    public class Startup
    {
        private FileCache _cache = null;

        public void Configuration(IAppBuilder app)
        {
            var cachePath = RoleEnvironment.IsAvailable ?
                RoleEnvironment.GetLocalResource("fileCache").RootPath :
                Path.Combine(Path.GetTempPath(), "PokerOdds.Web.OWIN");

            Directory.CreateDirectory(cachePath);

            _cache = new FileCache(cachePath);

            _cache.MaxCacheSize = RoleEnvironment.IsAvailable ?
                RoleEnvironment.GetLocalResource("fileCache").MaximumSizeInMegabytes * 1024 :
                2048 * 1024; //2GB

            _cache.MaxCacheSizeReached += MaxCacheSizeReached;

            PrimeCache();

            var router = new RequestRouter { Cache = _cache };

            app.Run(router.HandleRequest);
        }

        private void MaxCacheSizeReached(object sender, FileCacheEventArgs e)
        {
            //first try removing all non-completed items
            foreach (var odds in _cache.GetKeys()
                        .Select(key => _cache.GetCacheItem(key))
                        .Select(item => item.Value)
                        .OfType<TexasHoldemOdds>()
                        .Where(odds => !odds.Completed))
            {
                _cache.Remove(odds.GetCacheKey());
            }

            //if that has bought us some space
            if (_cache.GetCacheSize() < e.CurrentCacheSize)
            {
                //bail
                return;
            }

            //final last ditch approach
            //remove everything, and re-prime the cache
            _cache.Flush();
            PrimeCache();
        }

        private void PrimeCache()
        {
            Task.Factory.StartNew(() => {

                lock (_cache)
                {
                    var policy = new CacheItemPolicy { Priority = CacheItemPriority.NotRemovable };

                    using (var cacheZip = ZipFile.Read(Assembly.GetExecutingAssembly().GetManifestResourceStream("PokerOdds.Web.OWIN.Cache.PrimeCache.zip")))
                    {
                        foreach (var entry in cacheZip.Entries)
                        {
                            using (var reader = new StreamReader(entry.OpenReader()))
                            {
                                var contents = reader.ReadToEnd();

                                var odds = JsonConvert.DeserializeObject<TexasHoldemOdds>(contents);

                                var keys = new string[0];

                                try
                                {
                                    keys = _cache.GetKeys();
                                }
                                catch (Exception) { }

                                if (!keys.Contains(odds.GetCacheKey()))
                                {
                                    _cache.Add(odds.GetCacheKey(), odds, policy);
                                }
                            }
                        }
                    }
                }
            });
        }
    }
}