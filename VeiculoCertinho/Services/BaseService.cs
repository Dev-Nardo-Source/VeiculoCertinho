using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace VeiculoCertinho.Services
{
    public abstract class BaseService : IDisposable
    {
        protected readonly ILogger _logger;
        private static AdvancedCacheService? _globalCache;
        private static readonly object _cacheInitLock = new();

        protected BaseService(ILogger logger)
        {
            _logger = logger;
        }

        #region Advanced Cache Integration

        /// <summary>
        /// Cache avançado global para todos os serviços.
        /// </summary>
        protected static AdvancedCacheService GlobalCache
        {
            get
            {
                if (_globalCache == null)
                {
                    lock (_cacheInitLock)
                    {
                        _globalCache ??= new AdvancedCacheService(
                            Microsoft.Extensions.Logging.LoggerFactory.Create(builder => builder.AddConsole())
                                .CreateLogger<AdvancedCacheService>());
                    }
                }
                return _globalCache;
            }
        }

        /// <summary>
        /// Executa uma operação com cache automático avançado.
        /// </summary>
        protected async Task<T> ExecuteWithAdvancedCacheAsync<T>(
            string cacheKey, 
            Func<Task<T>> operation, 
            TimeSpan? expiry = null, 
            string[]? tags = null,
            string operationName = "",
            CancellationToken cancellationToken = default)
        {
            return await GlobalCache.GetOrSetAsync(
                cacheKey,
                async () =>
                {
                    using var activity = StartOperation(operationName);
                    return await ExecuteWithErrorHandlingAsync(operation, operationName, cancellationToken);
                },
                expiry,
                tags,
                cancellationToken
            );
        }

        /// <summary>
        /// Invalida cache por padrão.
        /// </summary>
        protected async Task InvalidateCachePatternAsync(string pattern, CancellationToken cancellationToken = default)
        {
            await GlobalCache.RemoveByPatternAsync(pattern, cancellationToken);
        }

        /// <summary>
        /// Invalida cache por tags.
        /// </summary>
        protected async Task InvalidateCacheTagsAsync(string[] tags, CancellationToken cancellationToken = default)
        {
            await GlobalCache.RemoveByTagsAsync(tags, cancellationToken);
        }

        #endregion

        #region Performance Monitoring

        /// <summary>
        /// Inicia monitoramento de performance para uma operação.
        /// </summary>
        protected IDisposable StartOperation(string operationName, [CallerMemberName] string callerName = "")
        {
            var fullOperationName = string.IsNullOrEmpty(operationName) ? callerName : operationName;
            return new PerformanceTracker(_logger, fullOperationName);
        }

        /// <summary>
        /// Executa operação com monitoramento automático de performance.
        /// </summary>
        protected async Task<T> ExecuteWithPerformanceTrackingAsync<T>(
            Func<Task<T>> operation, 
            string operationName = "",
            CancellationToken cancellationToken = default,
            [CallerMemberName] string callerName = "")
        {
            var fullOperationName = string.IsNullOrEmpty(operationName) ? callerName : operationName;
            
            using var tracker = StartOperation(fullOperationName);
            return await ExecuteWithErrorHandlingAsync(operation, fullOperationName, cancellationToken);
        }

        #endregion

        #region Enhanced Error Handling

        /// <summary>
        /// Executa uma operação com tratamento de erro padronizado.
        /// </summary>
        protected async Task<T?> ExecuteWithErrorHandlingAsync<T>(Func<Task<T?>> operation, string operationName, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Iniciando operação: {OperationName}", operationName);
                var result = await operation();
                _logger.LogInformation("Operação concluída com sucesso: {OperationName}", operationName);
                return result;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Operação cancelada: {OperationName}", operationName);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante operação: {OperationName} - {ErrorMessage}", operationName, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Executa uma operação sem retorno com tratamento de erro padronizado.
        /// </summary>
        protected async Task ExecuteWithErrorHandlingAsync(Func<Task> operation, string operationName, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Iniciando operação: {OperationName}", operationName);
                await operation();
                _logger.LogInformation("Operação concluída com sucesso: {OperationName}", operationName);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Operação cancelada: {OperationName}", operationName);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante operação: {OperationName} - {ErrorMessage}", operationName, ex.Message);
                throw;
            }
        }

        #endregion

        #region Enhanced Retry Logic

        /// <summary>
        /// Executa uma operação com retry automático e backoff exponencial.
        /// </summary>
        protected async Task<T?> ExecuteWithRetryAsync<T>(
            Func<Task<T?>> operation, 
            string operationName, 
            int maxRetries = 3, 
            int baseDelayMs = 1000,
            double backoffMultiplier = 2.0,
            CancellationToken cancellationToken = default)
        {
            var attempt = 1;
            Exception? lastException = null;

            while (attempt <= maxRetries)
            {
                try
                {
                    _logger.LogInformation("Tentativa {Attempt}/{MaxRetries} para operação: {OperationName}", 
                        attempt, maxRetries, operationName);
                    
                    return await operation();
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning("Operação cancelada na tentativa {Attempt}: {OperationName}", attempt, operationName);
                    throw;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    _logger.LogWarning("Falha na tentativa {Attempt}/{MaxRetries} para operação: {OperationName} - {ErrorMessage}", 
                        attempt, maxRetries, operationName, ex.Message);

                    if (attempt < maxRetries)
                    {
                        var delay = (int)(baseDelayMs * Math.Pow(backoffMultiplier, attempt - 1));
                        await Task.Delay(delay, cancellationToken);
                    }

                    attempt++;
                }
            }

            _logger.LogError(lastException, "Todas as tentativas falharam para operação: {OperationName}", operationName);
            throw lastException!;
        }

        #endregion

        #region Batch Operations

        /// <summary>
        /// Executa operações em lote com controle de paralelismo.
        /// </summary>
        protected async Task<T[]> ExecuteBatchAsync<T>(
            IEnumerable<Func<Task<T>>> operations,
            int maxConcurrency = 4,
            string operationName = "Batch Operation",
            CancellationToken cancellationToken = default)
        {
            using var semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
            var tasks = operations.Select(async operation =>
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    return await operation();
                }
                finally
                {
                    semaphore.Release();
                }
            });

            _logger.LogInformation("Executando operação em lote: {OperationName} com concorrência máxima: {MaxConcurrency}", 
                operationName, maxConcurrency);

            var results = await Task.WhenAll(tasks);
            
            _logger.LogInformation("Operação em lote concluída: {OperationName}, {Count} operações processadas", 
                operationName, results.Length);

            return results;
        }

        /// <summary>
        /// Processa itens em lote com controle de tamanho e paralelismo.
        /// </summary>
        protected async Task<TResult[]> ProcessBatchAsync<TInput, TResult>(
            IEnumerable<TInput> items,
            Func<TInput, Task<TResult>> processor,
            int batchSize = 100,
            int maxConcurrency = 4,
            string operationName = "Process Batch",
            CancellationToken cancellationToken = default)
        {
            var itemList = items.ToList();
            var results = new List<TResult>();
            
            _logger.LogInformation("Processando {Count} itens em lotes de {BatchSize} com concorrência {MaxConcurrency}",
                itemList.Count, batchSize, maxConcurrency);

            for (int i = 0; i < itemList.Count; i += batchSize)
            {
                var batch = itemList.Skip(i).Take(batchSize);
                var batchOperations = batch.Select(item => () => processor(item));
                
                var batchResults = await ExecuteBatchAsync(batchOperations, maxConcurrency, 
                    $"{operationName} - Batch {i / batchSize + 1}", cancellationToken);
                    
                results.AddRange(batchResults);
            }

            return results.ToArray();
        }

        #endregion

        #region Validation Helpers

        /// <summary>
        /// Valida se um objeto não é nulo.
        /// </summary>
        protected void ValidateNotNull<T>(T? obj, string parameterName) where T : class
        {
            if (obj == null)
            {
                var ex = new ArgumentNullException(parameterName, $"O parâmetro {parameterName} não pode ser nulo");
                _logger.LogError("Validação falhou: {ParameterName} é nulo", parameterName);
                throw ex;
            }
        }

        /// <summary>
        /// Valida se uma string não é nula ou vazia.
        /// </summary>
        protected void ValidateNotNullOrEmpty(string? value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                var ex = new ArgumentException($"O parâmetro {parameterName} não pode ser nulo ou vazio", parameterName);
                _logger.LogError("Validação falhou: {ParameterName} é nulo ou vazio", parameterName);
                throw ex;
            }
        }

        /// <summary>
        /// Valida se um ID é válido (maior que zero).
        /// </summary>
        protected void ValidateId(int id, string parameterName = "id")
        {
            if (id <= 0)
            {
                var ex = new ArgumentException($"O {parameterName} deve ser maior que zero", parameterName);
                _logger.LogError("Validação falhou: {ParameterName} = {Value} (deve ser > 0)", parameterName, id);
                throw ex;
            }
        }

        /// <summary>
        /// Valida múltiplas condições de uma vez.
        /// </summary>
        protected void ValidateConditions(params (bool condition, string message)[] validations)
        {
            foreach (var (condition, message) in validations)
            {
                if (!condition)
                {
                    _logger.LogError("Validação falhou: {Message}", message);
                    throw new ArgumentException(message);
                }
            }
        }

        #endregion

        #region Logging Helpers

        /// <summary>
        /// Log padronizado para operações de CRUD.
        /// </summary>
        protected void LogCrudOperation(string operation, string entityName, object? identifier = null)
        {
            if (identifier != null)
            {
                _logger.LogInformation("Operação {Operation} para {EntityName} com identificador: {Identifier}", 
                    operation, entityName, identifier);
            }
            else
            {
                _logger.LogInformation("Operação {Operation} para {EntityName}", operation, entityName);
            }
        }

        /// <summary>
        /// Log padronizado para sucessos de operações de CRUD.
        /// </summary>
        protected void LogCrudSuccess(string operation, string entityName, object? identifier = null, int? count = null)
        {
            if (count.HasValue)
            {
                _logger.LogInformation("Operação {Operation} para {EntityName} concluída com sucesso. Total processado: {Count}", 
                    operation, entityName, count.Value);
            }
            else if (identifier != null)
            {
                _logger.LogInformation("Operação {Operation} para {EntityName} com identificador {Identifier} concluída com sucesso", 
                    operation, entityName, identifier);
            }
            else
            {
                _logger.LogInformation("Operação {Operation} para {EntityName} concluída com sucesso", operation, entityName);
            }
        }

        #endregion

        #region Legacy Cache Support (Backward Compatibility)

        /// <summary>
        /// Método helper para cache simples em memória (mantido para compatibilidade).
        /// </summary>
        private static readonly Dictionary<string, (object Value, DateTime ExpiryTime)> _legacyCache = new();
        private static readonly object _legacyCacheLock = new();

        protected T? GetFromCache<T>(string key)
        {
            return GlobalCache.GetAsync<T>(key).GetAwaiter().GetResult();
        }

        protected void SetCache<T>(string key, T value, TimeSpan? expiry = null)
        {
            GlobalCache.SetAsync(key, value, expiry).GetAwaiter().GetResult();
        }

        protected void ClearCache(string? keyPattern = null)
        {
            if (string.IsNullOrEmpty(keyPattern))
            {
                GlobalCache.ClearAsync().GetAwaiter().GetResult();
            }
            else
            {
                GlobalCache.RemoveByPatternAsync(keyPattern).GetAwaiter().GetResult();
            }
        }

        #endregion

        #region Dispose

        protected virtual void Dispose(bool disposing)
        {
            // Override em classes filhas se necessário
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }

    #region Performance Tracker

    internal class PerformanceTracker : IDisposable
    {
        private readonly ILogger _logger;
        private readonly string _operationName;
        private readonly Stopwatch _stopwatch;

        public PerformanceTracker(ILogger logger, string operationName)
        {
            _logger = logger;
            _operationName = operationName;
            _stopwatch = Stopwatch.StartNew();
            _logger.LogDebug("Iniciando tracking de performance: {OperationName}", operationName);
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            var elapsed = _stopwatch.Elapsed;
            
            var logLevel = elapsed.TotalSeconds switch
            {
                > 30 => LogLevel.Warning,
                > 10 => LogLevel.Information,
                _ => LogLevel.Debug
            };

            _logger.Log(logLevel, "Performance: {OperationName} concluída em {ElapsedMs}ms", 
                _operationName, elapsed.TotalMilliseconds);
        }
    }

    #endregion
}
