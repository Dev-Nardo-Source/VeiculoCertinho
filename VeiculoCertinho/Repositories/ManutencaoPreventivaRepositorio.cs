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
    public class ManutencaoPreventivaRepositorio : BaseRepositorio
    {
        private readonly FirebaseHelper _firebaseHelper;
        private readonly IConfiguration _configuration;

        public ManutencaoPreventivaRepositorio(IConfiguration configuration, ILogger<ManutencaoPreventivaRepositorio>? logger = null)
            : base(logger ?? NullLogger<ManutencaoPreventivaRepositorio>.Instance)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _firebaseHelper = new FirebaseHelper(configuration);
        }

        /// <summary>
        /// Obtém todas as manutenções preventivas do banco SQLite.
        /// </summary>
        /// <param name="cancellationToken">Token para cancelamento da operação assíncrona.</param>
        /// <returns>Lista de objetos ManutencaoPreventiva.</returns>
        public async Task<List<ManutencaoPreventiva>> ObterTodosAsync(CancellationToken cancellationToken = default)
        {
            var lista = new List<ManutencaoPreventiva>();

            try
            {
                using (var connection = await CriarConexaoAsync(cancellationToken))
                {
                    var command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        SELECT Id, VeiculoId, TipoManutencao, DataAgendada, QuilometragemAgendada, DataRealizada, QuilometragemRealizada, Checklist, Observacoes, FotosComprovantes
                        FROM ManutencoesPreventivas
                        ORDER BY DataAgendada ASC;
                    ";

                    using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                    {
                        while (await reader.ReadAsync(cancellationToken))
                        {
                            var manutencao = new ManutencaoPreventiva
                            {
                                Id = reader.GetInt32(0),
                                VeiculoId = reader.GetInt32(1),
                                TipoManutencao = reader.GetString(2),
                                DataAgendada = reader.GetDateTime(3),
                                QuilometragemAgendada = reader.GetInt32(4),
                                DataRealizada = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5),
                                QuilometragemRealizada = reader.IsDBNull(6) ? (int?)null : reader.GetInt32(6),
                                Checklist = reader.IsDBNull(7) ? default! : reader.GetString(7),
                                Observacoes = reader.IsDBNull(8) ? default! : reader.GetString(8),
                                FotosComprovantes = reader.IsDBNull(9) ? default! : reader.GetString(9)
                            };
                            lista.Add(manutencao);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter manutenções preventivas");
                throw new Exception("Erro ao obter manutenções preventivas: " + ex.Message, ex);
            }

            return lista;
        }

        /// <summary>
        /// Adiciona uma nova manutenção preventiva no banco SQLite.
        /// </summary>
        /// <param name="manutencao">Objeto ManutencaoPreventiva a ser adicionado.</param>
        /// <param name="cancellationToken">Token para cancelamento da operação assíncrona.</param>
        /// <returns>Task representando a operação assíncrona.</returns>
        public async Task AdicionarAsync(ManutencaoPreventiva manutencao, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var connection = await CriarConexaoAsync(cancellationToken))
                {
                    var command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        INSERT INTO ManutencoesPreventivas (VeiculoId, TipoManutencao, DataAgendada, QuilometragemAgendada, DataRealizada, QuilometragemRealizada, Checklist, Observacoes, FotosComprovantes)
                        VALUES ($veiculoId, $tipoManutencao, $dataAgendada, $quilometragemAgendada, $dataRealizada, $quilometragemRealizada, $checklist, $observacoes, $fotosComprovantes);
                    ";

                    command.Parameters.AddWithValue("$veiculoId", manutencao.VeiculoId);
                    command.Parameters.AddWithValue("$tipoManutencao", manutencao.TipoManutencao);
                    command.Parameters.AddWithValue("$dataAgendada", manutencao.DataAgendada);
                    command.Parameters.AddWithValue("$quilometragemAgendada", manutencao.QuilometragemAgendada);
                    command.Parameters.AddWithValue("$dataRealizada", manutencao.DataRealizada ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("$quilometragemRealizada", manutencao.QuilometragemRealizada ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("$checklist", manutencao.Checklist ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("$observacoes", manutencao.Observacoes ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("$fotosComprovantes", manutencao.FotosComprovantes ?? (object)DBNull.Value);

                    await command.ExecuteNonQueryAsync(cancellationToken);
                }

                try
                {
                    await _firebaseHelper.EnviarAsync<ManutencaoPreventiva>(manutencao.Id.ToString(), manutencao, "PUT");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao sincronizar manutenção preventiva com Firebase");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar manutenção preventiva");
                throw new Exception("Erro ao adicionar manutenção preventiva: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Atualiza uma manutenção preventiva existente no banco SQLite.
        /// </summary>
        /// <param name="manutencao">Objeto ManutencaoPreventiva a ser atualizado.</param>
        /// <param name="cancellationToken">Token para cancelamento da operação assíncrona.</param>
        /// <returns>Task representando a operação assíncrona.</returns>
        public async Task AtualizarAsync(ManutencaoPreventiva manutencao, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var connection = await CriarConexaoAsync(cancellationToken))
                {
                    var command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        UPDATE ManutencoesPreventivas
                        SET VeiculoId = $veiculoId,
                            TipoManutencao = $tipoManutencao,
                            DataAgendada = $dataAgendada,
                            QuilometragemAgendada = $quilometragemAgendada,
                            DataRealizada = $dataRealizada,
                            QuilometragemRealizada = $quilometragemRealizada,
                            Checklist = $checklist,
                            Observacoes = $observacoes,
                            FotosComprovantes = $fotosComprovantes
                        WHERE Id = $id;
                    ";

                    command.Parameters.AddWithValue("$veiculoId", manutencao.VeiculoId);
                    command.Parameters.AddWithValue("$tipoManutencao", manutencao.TipoManutencao);
                    command.Parameters.AddWithValue("$dataAgendada", manutencao.DataAgendada);
                    command.Parameters.AddWithValue("$quilometragemAgendada", manutencao.QuilometragemAgendada);
                    command.Parameters.AddWithValue("$dataRealizada", manutencao.DataRealizada ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("$quilometragemRealizada", manutencao.QuilometragemRealizada ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("$checklist", manutencao.Checklist ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("$observacoes", manutencao.Observacoes ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("$fotosComprovantes", manutencao.FotosComprovantes ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("$id", manutencao.Id);

                    await command.ExecuteNonQueryAsync(cancellationToken);
                }

                try
                {
                    await _firebaseHelper.EnviarAsync<ManutencaoPreventiva>(manutencao.Id.ToString(), manutencao, "PUT");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao sincronizar atualização de manutenção preventiva com Firebase");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar manutenção preventiva");
                throw new Exception("Erro ao atualizar manutenção preventiva: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Remove uma manutenção preventiva pelo ID no banco SQLite.
        /// </summary>
        /// <param name="id">ID da manutenção preventiva a ser removida.</param>
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
                        DELETE FROM ManutencoesPreventivas
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
                    _logger.LogError(ex, "Erro ao sincronizar remoção de manutenção preventiva com Firebase");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover manutenção preventiva");
                throw new Exception("Erro ao remover manutenção preventiva: " + ex.Message, ex);
            }
        }
    }
}
