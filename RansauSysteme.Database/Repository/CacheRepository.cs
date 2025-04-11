using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using RansauSysteme.Database.Connection;

namespace RansauSysteme.Database.Repository
{
    public partial class CacheRepository<T> : BaseRepository<T> where T : class
    {
        // Fields needed to have a Cache implementation logic
        private volatile ConcurrentDictionary<int, T> _cache;

        private readonly SemaphoreSlim _cacheLock;
        private readonly TimeSpan? _refreshInterval;
        protected DateTime _lastRefresh;
        protected volatile bool _isCacheInitialized;

        public CacheRepository(IDatabaseConnection databaseConnection, TimeSpan? refreshInterval = null,
            bool periodicRefresh = false, ILogger? logger = null) : base(databaseConnection, logger)
        {
            // Adding some cache logics so if needed the data are still in memory for a period
            _cache = new ConcurrentDictionary<int, T>();
            _cacheLock = new SemaphoreSlim(1, 1);
            _refreshInterval = refreshInterval;
            _lastRefresh = DateTime.MinValue;
            _isCacheInitialized = false;

            // The repository auto refresh the Cache at the refresh interval period
            //Will be true only if refresh interval is not null
            if (refreshInterval != null && periodicRefresh)
            {
                StartPeriodicRefresh();
            }
        }

        //*****************
        // Get Methods
        //*******************
        public override T? GetById(int id)
        {
            if (IsUsingCache())
            {
                return GetFromCache(id);
            }

            return base.GetById(id);
        }

        public override async Task<T?> GetByIdAsync(int id)
        {
            if (IsUsingCache())
            {
                return await GetFromCacheAsync(id);
            }

            return await base.GetByIdAsync(id);
        }

        public override IEnumerable<T> GetAll()
        {
            if (IsUsingCache())
            {
                return GetAllFromCache();
            }

            return base.GetAll();
        }

        public override async Task<IEnumerable<T>> GetAllAsync()
        {
            if (IsUsingCache())
            {
                return await GetAllFromCacheAsync();
            }

            return await base.GetAllAsync();
        }

        //*****************
        // Add Methods
        //*******************
        public override int Add(T entity)
        {
            var result = base.Add(entity);
            if (IsUsingCache()) { InvalidateCache(); }
            return result;
        }

        public override bool Add(IEnumerable<T> entities)
        {
            var result = base.Add(entities);
            if (IsUsingCache()) { InvalidateCache(); }
            return result;
        }

        public override async Task<int> AddAsync(T entity)
        {
            var result = await base.AddAsync(entity);
            if (IsUsingCache()) { InvalidateCache(); }
            return result;
        }

        public override async Task<bool> AddAsync(IEnumerable<T> entities)
        {
            var result = await base.AddAsync(entities);
            if (IsUsingCache()) { InvalidateCache(); }
            return result;
        }

        //**************
        // Update Methods
        //***************
        public override bool Update(T entity)
        {
            var result = base.Update(entity);
            if (IsUsingCache()) { InvalidateCache(); }
            return result;
        }

        public override bool Update(IEnumerable<T> entities)
        {
            var result = base.Update(entities);
            if (IsUsingCache()) { InvalidateCache(); }
            return result;
        }

        public override async Task<bool> UpdateAsync(T entity)
        {
            var result = await base.UpdateAsync(entity);
            if (IsUsingCache()) { InvalidateCache(); }
            return result;
        }

        public override async Task<bool> UpdateAsync(IEnumerable<T> entities)
        {
            var result = await base.UpdateAsync(entities);
            if (IsUsingCache()) { InvalidateCache(); }
            return result;
        }

        //**************
        // Delete Methods
        //***************
        public override bool Delete(T entity)
        {
            var result = base.Delete(entity);
            if (IsUsingCache()) { InvalidateCache(); }
            return result;
        }

        public override bool Delete(IEnumerable<T> entities)
        {
            var result = base.Delete(entities);
            if (IsUsingCache()) { InvalidateCache(); }
            return result;
        }

        public override async Task<bool> DeleteAsync(T entity)
        {
            var result = await base.DeleteAsync(entity);
            if (IsUsingCache()) { InvalidateCache(); }
            return result;
        }

        public override async Task<bool> DeleteAsync(IEnumerable<T> entities)
        {
            var result = await base.DeleteAsync(entities);
            if (IsUsingCache()) { InvalidateCache(); }
            return result;
        }

        //*********************
        // Private Methods for Cache implementation
        //*********************

        protected bool IsUsingCache() => _refreshInterval != null;

        private T? GetFromCache(int id)
        {
            if (ShouldRefresh())
            {
                RefreshCache();
            }

            _cache.TryGetValue(id, out T? entity);

            return entity;
        }

        private IEnumerable<T> GetAllFromCache()
        {
            if (ShouldRefresh())
            {
                RefreshCache();
            }

            return _cache.Values;
        }

        private async Task<T?> GetFromCacheAsync(int id)
        {
            if (ShouldRefresh())
            {
                await RefreshCacheAsync();
            }

            _cache.TryGetValue(id, out T? entity);

            return entity;
        }

        private async Task<IEnumerable<T>> GetAllFromCacheAsync()
        {
            if (ShouldRefresh())
            {
                await RefreshCacheAsync();
            }

            return _cache.Values;
        }

        protected void RefreshCache()
        {
            try
            {
                var entities = base.GetAll();

                InvalidateCache();

                foreach (var entity in entities)
                {
                    var id = GetKeyIntValue(entity);
                    _cache.TryAdd(id, entity);
                }

                _lastRefresh = DateTime.UtcNow;
                _isCacheInitialized = true;

                string message = $"Cache refreshed for entity {typeof(T).Name}. Items: {_cache.Count}";
                Logger?.LogDebug(message);
            }
            catch (Exception ex)
            {
                string message = $"Error refreshing cache for entity {typeof(T).Name}";
                Logger?.LogError(ex, message);
                throw;
            }
        }

        protected async Task RefreshCacheAsync()
        {
            try
            {
                await _cacheLock.WaitAsync();

                var entities = await base.GetAllAsync();
                var newCache = new ConcurrentDictionary<int, T>();

                foreach (var entity in entities)
                {
                    var id = GetKeyIntValue(entity);
                    newCache.TryAdd(id, entity);
                }

                // Atomic cache replacement
                Thread.MemoryBarrier();
                Interlocked.Exchange(ref _cache, newCache);

                _lastRefresh = DateTime.UtcNow;
                _isCacheInitialized = true;

                string message = $"Cache refreshed for entity {typeof(T).Name}. Items: {newCache.Count}";
                Logger?.LogDebug(message);
            }
            catch (Exception ex)
            {
                string message = $"Error refreshing cache for entity {typeof(T).Name}";
                Logger?.LogError(ex, message);
                throw;
            }
            finally
            {
                _cacheLock.Release();
            }
        }

        public void InvalidateCache()
        {
            _cache.Clear();
            _isCacheInitialized = false;
            _lastRefresh = DateTime.MinValue;
        }

        protected bool ShouldRefresh()
        {
            if (IsUsingCache())
            {
                return !_isCacheInitialized || DateTime.UtcNow - _lastRefresh > _refreshInterval;
            }

            return false;
        }

        private void StartPeriodicRefresh()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    // We are sure it refresh interval is not null so applying the << ! >>  operator
                    await Task.Delay(_refreshInterval!.Value);

                    try
                    {
                        if (ShouldRefresh())
                        {
                            await RefreshCacheAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger?.LogError(ex, "Error in periodic cache refresh");
                    }
                }
            });
        }
    }
}