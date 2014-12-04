using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using System.Web;

namespace PokerOdds.Web.OWIN.Cache
{
    public class FileBackedMemoryCache : ObjectCache
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

        #region Not Implemented

        public override object AddOrGetExisting(string key, object value, CacheItemPolicy policy, string regionName = null)
        {
            throw new NotImplementedException();
        }

        public override CacheItem AddOrGetExisting(CacheItem value, CacheItemPolicy policy)
        {
            throw new NotImplementedException();
        }

        public override object AddOrGetExisting(string key, object value, DateTimeOffset absoluteExpiration, string regionName = null)
        {
            throw new NotImplementedException();
        }

        public override bool Contains(string key, string regionName = null)
        {
            throw new NotImplementedException();
        }

        public override CacheEntryChangeMonitor CreateCacheEntryChangeMonitor(IEnumerable<string> keys, string regionName = null)
        {
            throw new NotImplementedException();
        }

        public override DefaultCacheCapabilities DefaultCacheCapabilities
        {
            get { throw new NotImplementedException(); }
        }

        public override long GetCount(string regionName = null)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public override IDictionary<string, object> GetValues(IEnumerable<string> keys, string regionName = null)
        {
            throw new NotImplementedException();
        }

        #endregion Not Implemented

        public override string Name { get { return _memoryCache.Name; } }

        public long MaxCacheSize { get { return _fileCache.MaxCacheSize; } set { _fileCache.MaxCacheSize = value; } }

        public event EventHandler<FileCacheEventArgs> MaxCacheSizeReached;

        public override object Get(string key, string regionName = null)
        {
            var item = _memoryCache.Get(key, regionName);

            if (item == null)
            {
                item = _fileCache.Get(key, regionName);
                _memoryCache.Set(key, item, new CacheItemPolicy(), regionName);
            }

            return item;
        }

        public override CacheItem GetCacheItem(string key, string regionName = null)
        {
            var item = _memoryCache.GetCacheItem(key, regionName);

            if (item == null)
            {
                item = _fileCache.GetCacheItem(key, regionName);
                _memoryCache.Set(item, new CacheItemPolicy());
            }

            return item;
        }

        public override object Remove(string key, string regionName = null)
        {
            var item = _memoryCache.Remove(key, regionName);
            
            _fileCache.Remove(key, regionName);

            return item;
        }

        public override void Set(string key, object value, CacheItemPolicy policy, string regionName = null)
        {
            _memoryCache.Set(key, value, policy, regionName);

            //queue up writing to file cache on another thread
            ThreadPool.QueueUserWorkItem(_ => _fileCache.Set(key, _memoryCache.Get(key), policy, regionName));
        }

        public override void Set(CacheItem item, CacheItemPolicy policy)
        {
            _memoryCache.Set(item, policy);

            //queue up writing to file cache on another thread
            ThreadPool.QueueUserWorkItem(_ => _fileCache.Set(_memoryCache.GetCacheItem(item.Key, item.RegionName), policy));
        }

        public override void Set(string key, object value, DateTimeOffset absoluteExpiration, string regionName = null)
        {
            _memoryCache.Set(key, value, absoluteExpiration, regionName);

            //queue up writing to file cache on another thread
            ThreadPool.QueueUserWorkItem(_ => _fileCache.Set(key, _memoryCache.Get(key), absoluteExpiration, regionName));
        }

        public override object this[string key]
        {
            get { return Get(key); }
            set { Set(key, value, new CacheItemPolicy()); }
        }

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