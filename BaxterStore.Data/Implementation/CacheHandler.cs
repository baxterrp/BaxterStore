using BaxterStore.Data.Interfaces;
using BaxterStore.Data.POCOs;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;

namespace BaxterStore.Data.Implementation
{
    public class CacheHandler<TDataEntity> : ICacheHandler<TDataEntity> where TDataEntity : DataEntity
    {
        private IMemoryCache _memoryCache;
        private ILogger<CacheHandler<TDataEntity>> _logger;

        public CacheHandler(IMemoryCache memoryCache, ILogger<CacheHandler<TDataEntity>> logger)
        {
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void AddToCache(TDataEntity dataEntity)
        {
            _logger.LogTrace("Adding entity to cache with key {id} to expire in 60 seconds", dataEntity.Id);
            _memoryCache.Set(dataEntity.Id, dataEntity, new DateTimeOffset(DateTime.Now.AddMinutes(1)));
        }

        public void DropFromCache(string entityId)
        {
            if (_memoryCache.TryGetValue(entityId, out TDataEntity cachedDataEntity))
            {
                _logger.LogTrace("Found cached data entity with key {id}", entityId);

                _logger.LogTrace("Removing cached data entity with key {id}", entityId);

                _memoryCache.Remove(entityId);
            }
            else
            {
                _logger.LogTrace("No cached data entity found with key {id}", entityId);
            }
        }

        public bool TryGetValue(string entityId, out TDataEntity dataEntity)
        {
            _logger.LogTrace("Looking up cached data entity with key {id}", entityId);
            if(_memoryCache.TryGetValue(entityId, out TDataEntity cachedDataEntity) && cachedDataEntity != null)
            {
                _logger.LogTrace("Found cached data entity with key {id}", entityId);
                dataEntity = cachedDataEntity;
                return true;
            }

            _logger.LogTrace("No cached data entity found with key {id}", entityId);
            dataEntity = null;
            return false;
        }
    }
}
