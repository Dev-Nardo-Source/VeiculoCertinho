using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VeiculoCertinho.Models;
using Microsoft.Data.Sqlite;
using System;
using VeiculoCertinho.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace VeiculoCertinho.Repositories
{
    public class DashboardRepositorio : BaseRepositorio
    {
        private readonly FirebaseHelper _firebaseHelper;
        private readonly IConfiguration _configuration;

        public DashboardRepositorio(IConfiguration configuration, ILogger<DashboardRepositorio>? logger = null)
            : base(logger ?? NullLogger<DashboardRepositorio>.Instance)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _firebaseHelper = new FirebaseHelper(configuration);
        }

        /// <summary>
        /// Obtém todos os indicadores do dashboard do banco SQLite.
        /// </summary>
        /// <param name="cancellationToken">Token para cancelamento da operação assíncrona.</param>
        /// <returns>Lista de objetos DashboardIndicador.</returns>
        public async Task<List<DashboardIndicador>> ObterTodosAsync(CancellationToken cancellationToken = default)
        {
            var lista = new List<DashboardIndicador>();

            try
            {
                using (var connection = await CriarConexaoAsync(cancellationToken))
                {
                    var command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        SELECT Id, NomeIndicador, Valor, DataReferencia, Descricao
                        FROM DashboardIndicadores
                        ORDER BY DataReferencia DESC;
                    ";

                    using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                    {
                        while (await reader.ReadAsync(cancellationToken))
                        {
                            var indicador = new DashboardIndicador
                            {
                                Id = reader.GetInt32(0),
                                NomeIndicador = reader.GetString(1),
                                Valor = reader.GetDouble(2),
                                DataReferencia = reader.GetDateTime(3),
                                Descricao = reader.IsDBNull(4) ? default! : reader.GetString(4)
                            };
                            lista.Add(indicador);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter indicadores do dashboard");
                throw new Exception("Erro ao obter indicadores do dashboard: " + ex.Message, ex);
            }

            return lista;
        }

        /// <summary>
        /// Adiciona um novo indicador do dashboard no banco SQLite.
        /// </summary>
        /// <param name="indicador">Objeto DashboardIndicador a ser adicionado.</param>
        /// <param name="cancellationToken">Token para cancelamento da operação assíncrona.</param>
        /// <returns>Task representando a operação assíncrona.</returns>
        public async Task AdicionarAsync(DashboardIndicador indicador, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var connection = await CriarConexaoAsync(cancellationToken))
                {
                    var command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        INSERT INTO DashboardIndicadores (NomeIndicador, Valor, DataReferencia, Descricao)
                        VALUES ($nomeIndicador, $valor, $dataReferencia, $descricao);
                    ";

                    command.Parameters.AddWithValue("$nomeIndicador", indicador.NomeIndicador);
                    command.Parameters.AddWithValue("$valor", indicador.Valor);
                    command.Parameters.AddWithValue("$dataReferencia", indicador.DataReferencia);
                    command.Parameters.AddWithValue("$descricao", indicador.Descricao ?? (object)DBNull.Value);

                    await command.ExecuteNonQueryAsync(cancellationToken);
                }

                try
                {
                    await _firebaseHelper.EnviarAsync<DashboardIndicador>(indicador.Id.ToString(), indicador, "PUT");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao sincronizar indicador do dashboard com Firebase");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar indicador do dashboard");
                throw new Exception("Erro ao adicionar indicador do dashboard: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Atualiza um indicador do dashboard existente no banco SQLite.
        /// </summary>
        /// <param name="indicador">Objeto DashboardIndicador a ser atualizado.</param>
        /// <param name="cancellationToken">Token para cancelamento da operação assíncrona.</param>
        /// <returns>Task representando a operação assíncrona.</returns>
        public async Task AtualizarAsync(DashboardIndicador indicador, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var connection = await CriarConexaoAsync(cancellationToken))
                {
                    var command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        UPDATE DashboardIndicadores
                        SET NomeIndicador = $nomeIndicador,
                            Valor = $valor,
                            DataReferencia = $dataReferencia,
                            Descricao = $descricao
                        WHERE Id = $id;
                    ";

                    command.Parameters.AddWithValue("$nomeIndicador", indicador.NomeIndicador);
                    command.Parameters.AddWithValue("$valor", indicador.Valor);
                    command.Parameters.AddWithValue("$dataReferencia", indicador.DataReferencia);
                    command.Parameters.AddWithValue("$descricao", indicador.Descricao ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("$id", indicador.Id);

                    await command.ExecuteNonQueryAsync(cancellationToken);
                }

                try
                {
                    await _firebaseHelper.EnviarAsync<DashboardIndicador>(indicador.Id.ToString(), indicador, "PUT");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao sincronizar atualização do indicador do dashboard com Firebase");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar indicador do dashboard");
                throw new Exception("Erro ao atualizar indicador do dashboard: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Remove um indicador do dashboard pelo ID no banco SQLite.
        /// </summary>
        /// <param name="id">ID do indicador a ser removido.</param>
        /// <param name="cancellationToken">Token para cancelamento da operação assíncrona.</param>
        /// <returns>Task representando a operação assíncrona.</returns>
        public async Task RemoverAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var connection = await CriarConexaoAsync(cancellationToken))
                {
                    var command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        DELETE FROM DashboardIndicadores
                        WHERE Id = $id;
                    ";

                    command.Parameters.AddWithValue("$id", id);

                    await command.ExecuteNonQueryAsync(cancellationToken);
                }

                try
                {
                    await _firebaseHelper.EnviarAsync<object>(id.ToString(), default!, "DELETE");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao sincronizar remoção do indicador do dashboard com Firebase");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover indicador do dashboard");
                throw new Exception("Erro ao remover indicador do dashboard: " + ex.Message, ex);
            }
        }
    }
}
