using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using System.Web;

namespace PokerOdds.Web.OWIN.Cache
{
    public abstract class MultiStageCache<TPrimary, TSecondary> : ObjectCache
        where TPrimary: ObjectCache
        where TSecondary: ObjectCache
    {

        protected abstract TPrimary PrimaryCache { get; }
        protected abstract TSecondary SecondaryCache { get; }

        public override CacheEntryChangeMonitor CreateCacheEntryChangeMonitor(IEnumerable<string> keys, string regionName = null)
        {
            if ((PrimaryCache.DefaultCacheCapabilities & DefaultCacheCapabilities.CacheEntryChangeMonitors) == DefaultCacheCapabilities.CacheEntryChangeMonitors)
            {
                return PrimaryCache.CreateCacheEntryChangeMonitor(keys, regionName);
            }

            if ((SecondaryCache.DefaultCacheCapabilities & DefaultCacheCapabilities.CacheEntryChangeMonitors) == DefaultCacheCapabilities.CacheEntryChangeMonitors)
            {
                return SecondaryCache.CreateCacheEntryChangeMonitor(keys, regionName);
            }

            throw new NotImplementedException();
        }

        public override DefaultCacheCapabilities DefaultCacheCapabilities
        {
            get { return PrimaryCache.DefaultCacheCapabilities | SecondaryCache.DefaultCacheCapabilities; }
        }

        protected override IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return PrimaryCache.AsEnumerable().GetEnumerator();
        }

        public override IDictionary<string, object> GetValues(IEnumerable<string> keys, string regionName = null)
        {
            return PrimaryCache.GetValues(keys, regionName);
        }

        public override long GetCount(string regionName = null)
        {
            return PrimaryCache.GetCount(regionName);
        }

        public override object AddOrGetExisting(string key, object value, CacheItemPolicy policy, string regionName = null)
        {
            var item = PrimaryCache.AddOrGetExisting(key, value, policy, regionName);

            //queue up writing to secondary cache on another thread
            ThreadPool.QueueUserWorkItem(_ => SecondaryCache.AddOrGetExisting(key, PrimaryCache.Get(key), policy, regionName));

            return item;
        }

        public override CacheItem AddOrGetExisting(CacheItem value, CacheItemPolicy policy)
        {
            var item = PrimaryCache.AddOrGetExisting(value, policy);

            //queue up writing to secondary cache on another thread
            ThreadPool.QueueUserWorkItem(_ => SecondaryCache.AddOrGetExisting(PrimaryCache.GetCacheItem(value.Key), policy));

            return item;
        }

        public override object AddOrGetExisting(string key, object value, DateTimeOffset absoluteExpiration, string regionName = null)
        {
            var item = PrimaryCache.AddOrGetExisting(key, value, absoluteExpiration, regionName);

            //queue up writing to secondary cache on another thread
            ThreadPool.QueueUserWorkItem(_ => SecondaryCache.AddOrGetExisting(key, PrimaryCache.Get(key), absoluteExpiration, regionName));

            return item;
        }

        public override bool Contains(string key, string regionName = null)
        {
            return PrimaryCache.Contains(key, regionName) || SecondaryCache.Contains(key, regionName);
        }

        public override object Get(string key, string regionName = null)
        {
            var item = PrimaryCache.Get(key, regionName);

            if (item == null)
            {
                item = SecondaryCache.Get(key, regionName);

                if (item != null)
                    PrimaryCache.Set(key, item, new CacheItemPolicy(), regionName);
            }

            return item;
        }

        public override CacheItem GetCacheItem(string key, string regionName = null)
        {
            var item = PrimaryCache.GetCacheItem(key, regionName);

            if (item == null)
            {
                item = SecondaryCache.GetCacheItem(key, regionName);

                if (item != null)
                    PrimaryCache.Set(item, new CacheItemPolicy());
            }

            return item;
        }

        public override object Remove(string key, string regionName = null)
        {
            var item = PrimaryCache.Remove(key, regionName);

            SecondaryCache.Remove(key, regionName);

            return item;
        }

        public override void Set(string key, object value, CacheItemPolicy policy, string regionName = null)
        {
            PrimaryCache.Set(key, value, policy, regionName);

            //queue up writing to secondary cache on another thread
            ThreadPool.QueueUserWorkItem(_ => SecondaryCache.Set(key, PrimaryCache.Get(key), policy, regionName));
        }

        public override void Set(CacheItem item, CacheItemPolicy policy)
        {
            PrimaryCache.Set(item, policy);

            //queue up writing to secondary cache on another thread
            ThreadPool.QueueUserWorkItem(_ => SecondaryCache.Set(PrimaryCache.GetCacheItem(item.Key, item.RegionName), policy));
        }

        public override void Set(string key, object value, DateTimeOffset absoluteExpiration, string regionName = null)
        {
            PrimaryCache.Set(key, value, absoluteExpiration, regionName);

            //queue up writing to secondary cache on another thread
            ThreadPool.QueueUserWorkItem(_ => SecondaryCache.Set(key, PrimaryCache.Get(key), absoluteExpiration, regionName));
        }

        public override object this[string key]
        {
            get { return Get(key); }
            set { Set(key, value, new CacheItemPolicy()); }
        }

    }
}