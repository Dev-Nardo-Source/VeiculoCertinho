using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Linq;

namespace VeiculoCertinho.Services
{
    /// <summary>
    /// Sistema de cache avançado e inteligente para melhorar performance da aplicação.
    /// Substitui o cache simples com features como expiração automática, invalidação por padrões e estatísticas.
    /// </summary>
    public class AdvancedCacheService : BaseService
    {
        #region Cache Item Structure

        private class CacheItem<T>
        {
            public T Value { get; set; }
            public DateTime ExpiryTime { get; set; }
            public DateTime CreatedTime { get; set; }
            public int AccessCount { get; set; }
            public DateTime LastAccessTime { get; set; }
            public string[] Tags { get; set; } = Array.Empty<string>();
            
            public bool IsExpired => DateTime.UtcNow > ExpiryTime;
            public TimeSpan Age => DateTime.UtcNow - CreatedTime;
            public bool IsStale => Age.TotalMinutes > 30; // Configurable
        }

        #endregion

        #region Properties and Fields

        private readonly ConcurrentDictionary<string, object> _cache = new();
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();
        private readonly Timer _cleanupTimer;
        private readonly object _statsLock = new();
        
        // Statistics
        private long _totalRequests;
        private long _cacheHits;
        private long _cacheMisses;
        private long _totalEvictions;
        private long _totalSize;

        // Configuration
        private readonly TimeSpan _defaultExpiry = TimeSpan.FromMinutes(30);
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(5);
        private readonly int _maxCacheSize = 10000;

        #endregion

        #region Constructor

        public AdvancedCacheService(ILogger<AdvancedCacheService> logger) : base(logger)
        {
            _cleanupTimer = new Timer(CleanupExpiredItems, null, _cleanupInterval, _cleanupInterval);
            _logger.LogInformation("AdvancedCacheService inicializado com limpeza automática a cada {Interval} minutos", _cleanupInterval.TotalMinutes);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Obtém um item do cache de forma assíncrona.
        /// </summary>
        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            ValidateNotNullOrEmpty(key, nameof(key));
            
            Interlocked.Increment(ref _totalRequests);

            if (_cache.TryGetValue(key, out var cachedObject) && cachedObject is CacheItem<T> item)
            {
                if (!item.IsExpired)
                {
                    item.AccessCount++;
                    item.LastAccessTime = DateTime.UtcNow;
                    
                    Interlocked.Increment(ref _cacheHits);
                    _logger.LogDebug("Cache HIT para chave: {Key}, acessos: {AccessCount}", key, item.AccessCount);
                    
                    return item.Value;
                }
                else
                {
                    // Item expirado, remover
                    _cache.TryRemove(key, out _);
                    _locks.TryRemove(key, out _);
                    Interlocked.Increment(ref _totalEvictions);
                }
            }

            Interlocked.Increment(ref _cacheMisses);
            _logger.LogDebug("Cache MISS para chave: {Key}", key);
            return default;
        }

        /// <summary>
        /// Obtém um item do cache ou executa uma função para buscá-lo.
        /// </summary>
        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null, string[]? tags = null, CancellationToken cancellationToken = default)
        {
            var cached = await GetAsync<T>(key, cancellationToken);
            if (cached != null && !cached.Equals(default(T)))
            {
                return cached;
            }

            // Usar semáforo para evitar múltiplas execuções da mesma factory
            var lockKey = $"lock_{key}";
            var semaphore = _locks.GetOrAdd(lockKey, _ => new SemaphoreSlim(1, 1));

            await semaphore.WaitAsync(cancellationToken);
            try
            {
                // Verificar novamente após obter o lock
                cached = await GetAsync<T>(key, cancellationToken);
                if (cached != null && !cached.Equals(default(T)))
                {
                    return cached;
                }

                // Executar factory e cachear resultado
                var value = await factory();
                await SetAsync(key, value, expiry, tags, cancellationToken);
                
                _logger.LogDebug("Valor calculado e cacheado para chave: {Key}", key);
                return value;
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <summary>
        /// Define um item no cache.
        /// </summary>
        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, string[]? tags = null, CancellationToken cancellationToken = default)
        {
            ValidateNotNullOrEmpty(key, nameof(key));

            var expiryTime = DateTime.UtcNow.Add(expiry ?? _defaultExpiry);
            var cacheItem = new CacheItem<T>
            {
                Value = value,
                ExpiryTime = expiryTime,
                CreatedTime = DateTime.UtcNow,
                LastAccessTime = DateTime.UtcNow,
                AccessCount = 0,
                Tags = tags ?? Array.Empty<string>()
            };

            _cache.AddOrUpdate(key, cacheItem, (k, v) => cacheItem);
            Interlocked.Increment(ref _totalSize);

            // Verificar se precisa fazer limpeza por tamanho
            if (_cache.Count > _maxCacheSize)
            {
                await Task.Run(() => EvictLeastRecentlyUsed(), cancellationToken);
            }

            _logger.LogDebug("Item adicionado ao cache: {Key}, expira em: {ExpiryTime}", key, expiryTime);
        }

        /// <summary>
        /// Remove um item específico do cache.
        /// </summary>
        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            ValidateNotNullOrEmpty(key, nameof(key));

            if (_cache.TryRemove(key, out _))
            {
                _locks.TryRemove($"lock_{key}", out _);
                Interlocked.Increment(ref _totalEvictions);
                _logger.LogDebug("Item removido do cache: {Key}", key);
            }
        }

        /// <summary>
        /// Remove itens do cache por padrão de chave.
        /// </summary>
        public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
        {
            ValidateNotNullOrEmpty(pattern, nameof(pattern));

            var keysToRemove = _cache.Keys.Where(k => k.Contains(pattern, StringComparison.OrdinalIgnoreCase)).ToList();
            
            foreach (var key in keysToRemove)
            {
                await RemoveAsync(key, cancellationToken);
            }

            _logger.LogInformation("Removidos {Count} itens do cache por padrão: {Pattern}", keysToRemove.Count, pattern);
        }

        /// <summary>
        /// Remove itens do cache por tags.
        /// </summary>
        public async Task RemoveByTagsAsync(string[] tags, CancellationToken cancellationToken = default)
        {
            if (tags == null || tags.Length == 0) return;

            var keysToRemove = new List<string>();

            foreach (var kvp in _cache)
            {
                if (kvp.Value is CacheItem<object> item && item.Tags.Any(tag => tags.Contains(tag, StringComparer.OrdinalIgnoreCase)))
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                await RemoveAsync(key, cancellationToken);
            }

            _logger.LogInformation("Removidos {Count} itens do cache por tags: {Tags}", keysToRemove.Count, string.Join(", ", tags));
        }

        /// <summary>
        /// Limpa todo o cache.
        /// </summary>
        public async Task ClearAsync(CancellationToken cancellationToken = default)
        {
            var count = _cache.Count;
            _cache.Clear();
            _locks.Clear();
            
            Interlocked.Add(ref _totalEvictions, count);
            Interlocked.Exchange(ref _totalSize, 0);
            
            _logger.LogInformation("Cache completamente limpo: {Count} itens removidos", count);
        }

        /// <summary>
        /// Obtém estatísticas do cache.
        /// </summary>
        public CacheStatistics GetStatistics()
        {
            lock (_statsLock)
            {
                var hitRate = _totalRequests > 0 ? (double)_cacheHits / _totalRequests * 100 : 0;
                
                return new CacheStatistics
                {
                    TotalRequests = _totalRequests,
                    CacheHits = _cacheHits,
                    CacheMisses = _cacheMisses,
                    HitRate = hitRate,
                    TotalEvictions = _totalEvictions,
                    CurrentSize = _cache.Count,
                    TotalSize = _totalSize,
                    MaxSize = _maxCacheSize
                };
            }
        }

        #endregion

        #region Private Methods

        private void CleanupExpiredItems(object? state)
        {
            try
            {
                var expiredKeys = new List<string>();
                var now = DateTime.UtcNow;

                foreach (var kvp in _cache)
                {
                    if (kvp.Value is CacheItem<object> item && item.IsExpired)
                    {
                        expiredKeys.Add(kvp.Key);
                    }
                }

                foreach (var key in expiredKeys)
                {
                    _cache.TryRemove(key, out _);
                    _locks.TryRemove($"lock_{key}", out _);
                }

                if (expiredKeys.Count > 0)
                {
                    Interlocked.Add(ref _totalEvictions, expiredKeys.Count);
                    _logger.LogDebug("Limpeza automática: {Count} itens expirados removidos", expiredKeys.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante limpeza automática do cache");
            }
        }

        private void EvictLeastRecentlyUsed()
        {
            try
            {
                var itemsToEvict = _cache
                    .Where(kvp => kvp.Value is CacheItem<object>)
                    .Select(kvp => new { Key = kvp.Key, Item = (CacheItem<object>)kvp.Value })
                    .OrderBy(x => x.Item.LastAccessTime)
                    .Take(_cache.Count - (_maxCacheSize * 80 / 100)) // Remove 20% do cache
                    .Select(x => x.Key)
                    .ToList();

                foreach (var key in itemsToEvict)
                {
                    _cache.TryRemove(key, out _);
                    _locks.TryRemove($"lock_{key}", out _);
                }

                Interlocked.Add(ref _totalEvictions, itemsToEvict.Count);
                _logger.LogInformation("Eviction LRU: {Count} itens removidos para liberar espaço", itemsToEvict.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante eviction LRU");
            }
        }

        #endregion

        #region Dispose

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _cleanupTimer?.Dispose();
                
                foreach (var semaphore in _locks.Values)
                {
                    semaphore?.Dispose();
                }
                
                _cache.Clear();
                _locks.Clear();
            }
            
            base.Dispose(disposing);
        }

        #endregion
    }

    #region Statistics Model

    public class CacheStatistics
    {
        public long TotalRequests { get; set; }
        public long CacheHits { get; set; }
        public long CacheMisses { get; set; }
        public double HitRate { get; set; }
        public long TotalEvictions { get; set; }
        public int CurrentSize { get; set; }
        public long TotalSize { get; set; }
        public int MaxSize { get; set; }
        
        public override string ToString()
        {
            return $"Cache Stats - Requests: {TotalRequests}, Hits: {CacheHits}, Misses: {CacheMisses}, Hit Rate: {HitRate:F2}%, Size: {CurrentSize}/{MaxSize}";
        }
    }

    #endregion
} 