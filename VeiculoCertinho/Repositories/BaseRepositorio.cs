using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using VeiculoCertinho.Database;

namespace VeiculoCertinho.Repositories
{
    public class BaseRepositorio : IBaseRepositorio
    {
        protected readonly ILogger _logger;

        public BaseRepositorio(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Cria e abre uma conexão SQLite.
        /// </summary>
        /// <param name="cancellationToken">Token para cancelamento da operação.</param>
        /// <returns>Conexão SQLite aberta.</returns>
        protected async Task<SqliteConnection> CriarConexaoAsync(CancellationToken cancellationToken = default)
        {
            var connection = DatabaseConfig.GetConnection();
            await connection.OpenAsync(cancellationToken);
            return connection;
        }

        /// <summary>
        /// Executa um script SQL no banco de dados.
        /// </summary>
        /// <param name="scriptSql">Script SQL a ser executado.</param>
        /// <param name="cancellationToken">Token para cancelamento da operação.</param>
        public async Task ExecutarScriptSqlAsync(string scriptSql, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var connection = await CriarConexaoAsync(cancellationToken))
                {
                    var comandos = scriptSql.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var comando in comandos)
                    {
                        var command = connection.CreateCommand();
                        command.CommandText = comando.Trim();
                        if (!string.IsNullOrWhiteSpace(command.CommandText))
                        {
                            _logger.LogInformation($"Executando comando SQL: {command.CommandText}");
                            await command.ExecuteNonQueryAsync(cancellationToken);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao executar script SQL");
                throw;
            }
        }

        /// <summary>
        /// Executa uma query e mapeia os resultados usando um mapper customizado.
        /// </summary>
        protected async Task<List<T>> ExecuteQueryAsync<T>(string query, Func<IDataReader, T> mapper, 
            Dictionary<string, object>? parameters = null, CancellationToken cancellationToken = default)
        {
            var lista = new List<T>();

            try
            {
                using (var connection = await CriarConexaoAsync(cancellationToken))
                {
                    var command = connection.CreateCommand();
                    command.CommandText = query;

                    // Adicionar parâmetros se fornecidos
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            command.Parameters.AddWithValue($"${param.Key}", param.Value ?? DBNull.Value);
                        }
                    }

                    using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                    {
                        while (await reader.ReadAsync(cancellationToken))
                        {
                            lista.Add(mapper(reader));
                        }
                    }
                }

                return lista;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao executar query: {Query}", query);
                throw;
            }
        }

        /// <summary>
        /// Executa uma query que retorna um único resultado.
        /// </summary>
        protected async Task<T?> ExecuteQuerySingleAsync<T>(string query, Func<IDataReader, T> mapper, 
            Dictionary<string, object>? parameters = null, CancellationToken cancellationToken = default) where T : class
        {
            try
            {
                using (var connection = await CriarConexaoAsync(cancellationToken))
                {
                    var command = connection.CreateCommand();
                    command.CommandText = query;

                    // Adicionar parâmetros se fornecidos
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            command.Parameters.AddWithValue($"${param.Key}", param.Value ?? DBNull.Value);
                        }
                    }

                    using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                    {
                        if (await reader.ReadAsync(cancellationToken))
                        {
                            return mapper(reader);
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao executar query single: {Query}", query);
                throw;
            }
        }

        /// <summary>
        /// Executa uma query que retorna um único resultado para tipos valor (struct).
        /// </summary>
        protected async Task<T?> ExecuteQuerySingleStructAsync<T>(string query, Func<IDataReader, T> mapper, 
            Dictionary<string, object>? parameters = null, CancellationToken cancellationToken = default) where T : struct
        {
            try
            {
                using (var connection = await CriarConexaoAsync(cancellationToken))
                {
                    var command = connection.CreateCommand();
                    command.CommandText = query;

                    // Adicionar parâmetros se fornecidos
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            command.Parameters.AddWithValue($"${param.Key}", param.Value ?? DBNull.Value);
                        }
                    }

                    using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                    {
                        if (await reader.ReadAsync(cancellationToken))
                        {
                            return mapper(reader);
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao executar query single struct: {Query}", query);
                throw;
            }
        }

        /// <summary>
        /// Executa um comando SQL (INSERT, UPDATE, DELETE).
        /// </summary>
        protected async Task<int> ExecuteCommandAsync(string commandSql, Dictionary<string, object>? parameters = null, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                using (var connection = await CriarConexaoAsync(cancellationToken))
                {
                    var command = connection.CreateCommand();
                    command.CommandText = commandSql;

                    // Adicionar parâmetros se fornecidos
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            command.Parameters.AddWithValue($"${param.Key}", param.Value ?? DBNull.Value);
                        }
                    }

                    return await command.ExecuteNonQueryAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao executar comando: {Command}", commandSql);
                throw;
            }
        }

        /// <summary>
        /// Executa um comando SQL que retorna um scalar (ex: COUNT, MAX, etc).
        /// </summary>
        protected async Task<T?> ExecuteScalarAsync<T>(string commandSql, Dictionary<string, object>? parameters = null, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                using (var connection = await CriarConexaoAsync(cancellationToken))
                {
                    var command = connection.CreateCommand();
                    command.CommandText = commandSql;

                    // Adicionar parâmetros se fornecidos
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            command.Parameters.AddWithValue($"${param.Key}", param.Value ?? DBNull.Value);
                        }
                    }

                    var result = await command.ExecuteScalarAsync(cancellationToken);
                    if (result == null || result == DBNull.Value)
                        return default;

                    return (T)Convert.ChangeType(result, typeof(T));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao executar scalar: {Command}", commandSql);
                throw;
            }
        }

        /// <summary>
        /// Helper para obter valores seguros do DataReader.
        /// </summary>
        protected static T GetSafeValue<T>(IDataReader reader, string columnName, T defaultValue = default)
        {
            try
            {
                var ordinal = reader.GetOrdinal(columnName);
                if (reader.IsDBNull(ordinal))
                    return defaultValue;

                var value = reader.GetValue(ordinal);
                if (value == null || value == DBNull.Value)
                    return defaultValue;

                if (typeof(T) == typeof(string))
                    return (T)(object)value.ToString()!;

                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Helper para obter valores seguros do DataReader por índice.
        /// </summary>
        protected static T GetSafeValue<T>(IDataReader reader, int index, T defaultValue = default)
        {
            try
            {
                if (reader.IsDBNull(index))
                    return defaultValue;

                var value = reader.GetValue(index);
                if (value == null || value == DBNull.Value)
                    return defaultValue;

                if (typeof(T) == typeof(string))
                    return (T)(object)value.ToString()!;

                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}
