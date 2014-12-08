using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using System.Web;

namespace PokerOdds.Web.OWIN.Cache
{
    public class FileBackedMemoryCache : MultiStageCache<MemoryCache, FileCache>
    {
        private readonly MemoryCache _memoryCache;
        private readonly FileCache _fileCache;

        public FileBackedMemoryCache(string name, string cacheRoot)
        {
            _memoryCache = new MemoryCache(name);
            _fileCache = new FileCache(cacheRoot);

            _fileCache.MaxCacheSizeReached += (s, e) => 
            {
                var maxCacheSizeReached = MaxCacheSizeReached;
                if (maxCacheSizeReached != null)
                {
                    maxCacheSizeReached(s, e);
                }
            };
        }

        protected override MemoryCache PrimaryCache { get { return _memoryCache; } }
        protected override FileCache SecondaryCache { get { return _fileCache; } }

        public override string Name { get { return _memoryCache.Name ?? _fileCache.Name; } }

        public long MaxCacheSize { get { return _fileCache.MaxCacheSize; } set { _fileCache.MaxCacheSize = value; } }

        public event EventHandler<FileCacheEventArgs> MaxCacheSizeReached;
 
        public string[] GetKeys(string regionName = null)
        {
            var keyList = _memoryCache.Select(item => item.Key).ToList();

            keyList.AddRange(_fileCache.GetKeys());

            return keyList.Distinct().ToArray();
        }

        public long GetCacheSize(string regionName = null)
        {
            return _fileCache.GetCacheSize(regionName);
        }

        public void Flush(string regionName = null)
        {
            _memoryCache.ToList().ForEach(kvp => _memoryCache.Remove(kvp.Key, regionName));
            _fileCache.Flush(regionName);
        }
    }
}