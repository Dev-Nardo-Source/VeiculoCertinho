using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace VeiculoCertinho.Utils
{
    /// <summary>
    /// Extensões para padronização de tratamento de erros em toda a aplicação.
    /// </summary>
    public static class ErrorHandlingExtensions
    {
        #region Exception Handling

        /// <summary>
        /// Executa uma operação com tratamento padronizado de exceções.
        /// </summary>
        public static async Task<T> ExecuteSafelyAsync<T>(
            this Func<Task<T>> operation,
            ILogger logger,
            T defaultValue = default,
            string operationName = "",
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string callerFile = "",
            [CallerLineNumber] int callerLine = 0)
        {
            var fullOperationName = string.IsNullOrEmpty(operationName) ? callerName : operationName;
            var context = new OperationContext(callerName, callerFile, callerLine);

            try
            {
                logger.LogDebug("Iniciando operação: {OperationName} em {Context}", fullOperationName, context);
                var result = await operation();
                logger.LogDebug("Operação concluída com sucesso: {OperationName}", fullOperationName);
                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro na operação: {OperationName} em {Context} - {ErrorMessage}", 
                    fullOperationName, context, ex.Message);
                
                return defaultValue;
            }
        }

        /// <summary>
        /// Executa uma operação sem retorno com tratamento padronizado de exceções.
        /// </summary>
        public static async Task ExecuteSafelyAsync(
            this Func<Task> operation,
            ILogger logger,
            string operationName = "",
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string callerFile = "",
            [CallerLineNumber] int callerLine = 0)
        {
            var fullOperationName = string.IsNullOrEmpty(operationName) ? callerName : operationName;
            var context = new OperationContext(callerName, callerFile, callerLine);

            try
            {
                logger.LogDebug("Iniciando operação: {OperationName} em {Context}", fullOperationName, context);
                await operation();
                logger.LogDebug("Operação concluída com sucesso: {OperationName}", fullOperationName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro na operação: {OperationName} em {Context} - {ErrorMessage}", 
                    fullOperationName, context, ex.Message);
                
                throw new OperationException(fullOperationName, ex, context);
            }
        }

        /// <summary>
        /// Executa uma operação síncrona com tratamento padronizado de exceções.
        /// </summary>
        public static T ExecuteSafely<T>(
            this Func<T> operation,
            ILogger logger,
            T defaultValue = default,
            string operationName = "",
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string callerFile = "",
            [CallerLineNumber] int callerLine = 0)
        {
            var fullOperationName = string.IsNullOrEmpty(operationName) ? callerName : operationName;
            var context = new OperationContext(callerName, callerFile, callerLine);

            try
            {
                logger.LogDebug("Iniciando operação: {OperationName} em {Context}", fullOperationName, context);
                var result = operation();
                logger.LogDebug("Operação concluída com sucesso: {OperationName}", fullOperationName);
                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro na operação: {OperationName} em {Context} - {ErrorMessage}", 
                    fullOperationName, context, ex.Message);
                
                return defaultValue;
            }
        }

        #endregion

        #region Database Error Handling

        /// <summary>
        /// Executa uma operação de banco de dados com tratamento específico de erros SQL.
        /// </summary>
        public static async Task<T> ExecuteDatabaseOperationAsync<T>(
            this Func<Task<T>> operation,
            ILogger logger,
            string operationName = "",
            T defaultValue = default,
            int retryCount = 3)
        {
            var attempt = 1;
            Exception lastException = null;

            while (attempt <= retryCount)
            {
                try
                {
                    logger.LogDebug("Tentativa {Attempt}/{MaxRetries} para operação de BD: {OperationName}", 
                        attempt, retryCount, operationName);
                    
                    return await operation();
                }
                catch (Microsoft.Data.Sqlite.SqliteException sqlEx)
                {
                    lastException = sqlEx;
                    logger.LogWarning("Erro SQLite na tentativa {Attempt}: {OperationName} - Código: {ErrorCode}, Mensagem: {ErrorMessage}", 
                        attempt, operationName, sqlEx.SqliteErrorCode, sqlEx.Message);

                    if (attempt < retryCount && IsSqliteRetryableError(sqlEx))
                    {
                        var delay = TimeSpan.FromMilliseconds(1000 * attempt);
                        await Task.Delay(delay);
                        attempt++;
                        continue;
                    }
                    
                    break;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    logger.LogError(ex, "Erro geral na operação de BD: {OperationName} - Tentativa {Attempt}", 
                        operationName, attempt);
                    break;
                }
            }

            logger.LogError(lastException, "Falha definitiva na operação de BD: {OperationName} após {Attempts} tentativas", 
                operationName, attempt - 1);
            
            return defaultValue;
        }

        /// <summary>
        /// Verifica se um erro SQLite pode ser tentado novamente.
        /// </summary>
        private static bool IsSqliteRetryableError(Microsoft.Data.Sqlite.SqliteException ex)
        {
            // Códigos de erro SQLite que indicam condições temporárias
            // SQLITE_BUSY = 5, SQLITE_LOCKED = 6, SQLITE_IOERR = 10
            return ex.SqliteErrorCode == 5 || ex.SqliteErrorCode == 6 || ex.SqliteErrorCode == 10;
        }

        #endregion

        #region Network Error Handling

        /// <summary>
        /// Executa uma operação de rede com tratamento específico de erros de conectividade.
        /// </summary>
        public static async Task<T> ExecuteNetworkOperationAsync<T>(
            this Func<Task<T>> operation,
            ILogger logger,
            string operationName = "",
            T defaultValue = default,
            int retryCount = 3)
        {
            var attempt = 1;
            Exception lastException = null;

            while (attempt <= retryCount)
            {
                try
                {
                    logger.LogDebug("Tentativa {Attempt}/{MaxRetries} para operação de rede: {OperationName}", 
                        attempt, retryCount, operationName);
                    
                    return await operation();
                }
                catch (System.Net.Http.HttpRequestException httpEx)
                {
                    lastException = httpEx;
                    logger.LogWarning("Erro HTTP na tentativa {Attempt}: {OperationName} - {ErrorMessage}", 
                        attempt, operationName, httpEx.Message);

                    if (attempt < retryCount)
                    {
                        var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt)); // Backoff exponencial
                        await Task.Delay(delay);
                        attempt++;
                        continue;
                    }
                    
                    break;
                }
                catch (TaskCanceledException tcEx) when (tcEx.InnerException is TimeoutException)
                {
                    lastException = tcEx;
                    logger.LogWarning("Timeout na tentativa {Attempt}: {OperationName}", attempt, operationName);

                    if (attempt < retryCount)
                    {
                        attempt++;
                        continue;
                    }
                    
                    break;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    logger.LogError(ex, "Erro geral na operação de rede: {OperationName} - Tentativa {Attempt}", 
                        operationName, attempt);
                    break;
                }
            }

            logger.LogError(lastException, "Falha definitiva na operação de rede: {OperationName} após {Attempts} tentativas", 
                operationName, attempt - 1);
            
            return defaultValue;
        }

        #endregion

        #region Validation Error Handling

        /// <summary>
        /// Valida uma operação e executa com tratamento de erros de validação.
        /// </summary>
        public static async Task<T> ExecuteWithValidationAsync<T>(
            this Func<Task<T>> operation,
            ILogger logger,
            Func<bool> validator,
            string validationMessage = "Validação falhou",
            string operationName = "",
            T defaultValue = default)
        {
            try
            {
                if (!validator())
                {
                    throw new ValidationException(validationMessage);
                }

                return await operation();
            }
            catch (ValidationException vex)
            {
                logger.LogWarning("Erro de validação na operação: {OperationName} - {ValidationMessage}", 
                    operationName, vex.Message);
                return defaultValue;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro na operação com validação: {OperationName} - {ErrorMessage}", 
                    operationName, ex.Message);
                return defaultValue;
            }
        }

        #endregion

        #region Error Logging Helpers

        /// <summary>
        /// Loga uma exceção com contexto estruturado.
        /// </summary>
        public static void LogStructuredError(
            this ILogger logger,
            Exception exception,
            string operationName,
            object? additionalData = null,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string callerFile = "",
            [CallerLineNumber] int callerLine = 0)
        {
            var context = new OperationContext(callerName, callerFile, callerLine);
            
            logger.LogError(exception, 
                "Erro estruturado: {OperationName} | Contexto: {Context} | Dados: {AdditionalData} | Exceção: {ExceptionType} - {ErrorMessage}",
                operationName, context, additionalData, exception.GetType().Name, exception.Message);
        }

        /// <summary>
        /// Loga uma operação de performance com warning se estiver lenta.
        /// </summary>
        public static void LogPerformanceWarning(
            this ILogger logger,
            TimeSpan elapsed,
            string operationName,
            TimeSpan warningThreshold)
        {
            if (elapsed > warningThreshold)
            {
                logger.LogWarning("Operação lenta detectada: {OperationName} - Tempo: {ElapsedMs}ms (Limite: {ThresholdMs}ms)",
                    operationName, elapsed.TotalMilliseconds, warningThreshold.TotalMilliseconds);
            }
        }

        #endregion
    }

    #region Custom Exceptions

    /// <summary>
    /// Exceção personalizada para operações com contexto adicional.
    /// </summary>
    public class OperationException : Exception
    {
        public string OperationName { get; }
        public OperationContext Context { get; }

        public OperationException(string operationName, Exception innerException, OperationContext context)
            : base($"Erro na operação '{operationName}': {innerException.Message}", innerException)
        {
            OperationName = operationName;
            Context = context;
        }
    }

    /// <summary>
    /// Exceção para erros de validação.
    /// </summary>
    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message) { }
        public ValidationException(string message, Exception innerException) : base(message, innerException) { }
    }

    #endregion

    #region Context Models

    /// <summary>
    /// Contexto de uma operação para logging estruturado.
    /// </summary>
    public readonly struct OperationContext
    {
        public string MethodName { get; }
        public string FileName { get; }
        public int LineNumber { get; }

        public OperationContext(string methodName, string fileName, int lineNumber)
        {
            MethodName = methodName;
            FileName = System.IO.Path.GetFileName(fileName);
            LineNumber = lineNumber;
        }

        public override string ToString()
        {
            return $"{MethodName} em {FileName}:{LineNumber}";
        }
    }

    #endregion
} 