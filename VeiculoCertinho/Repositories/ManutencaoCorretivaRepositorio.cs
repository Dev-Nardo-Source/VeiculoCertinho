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
    public class ManutencaoCorretivaRepositorio : BaseRepositorio
    {
        private readonly FirebaseHelper _firebaseHelper;
        private readonly IConfiguration _configuration;

        public ManutencaoCorretivaRepositorio(IConfiguration configuration, ILogger<ManutencaoCorretivaRepositorio>? logger = null)
            : base(logger ?? NullLogger<ManutencaoCorretivaRepositorio>.Instance)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _firebaseHelper = new FirebaseHelper(configuration);
        }

        /// <summary>
        /// Obtém todas as manutenções corretivas do banco SQLite.
        /// </summary>
        /// <param name="cancellationToken">Token para cancelamento da operação assíncrona.</param>
        /// <returns>Lista de objetos ManutencaoCorretiva.</returns>
        public async Task<List<ManutencaoCorretiva>> ObterTodosAsync(CancellationToken cancellationToken = default)
        {
            var lista = new List<ManutencaoCorretiva>();

            try
            {
                using (var connection = await CriarConexaoAsync(cancellationToken))
                {
                    var command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        SELECT Id, VeiculoId, DataRegistro, DescricaoFalha, Observacoes, SolucaoAplicada, FotosComprovantes
                        FROM ManutencoesCorretivas
                        ORDER BY DataRegistro ASC;
                    ";

                    using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                    {
                        while (await reader.ReadAsync(cancellationToken))
                        {
                            var manutencao = new ManutencaoCorretiva
                            {
                                Id = reader.GetInt32(0),
                                VeiculoId = reader.GetInt32(1),
                                DataRegistro = reader.GetDateTime(2),
                                DescricaoFalha = reader.GetString(3),
                                Observacoes = reader.IsDBNull(4) ? default! : reader.GetString(4),
                                SolucaoAplicada = reader.IsDBNull(5) ? default! : reader.GetString(5),
                                FotosComprovantes = reader.IsDBNull(6) ? default! : reader.GetString(6)
                            };
                            lista.Add(manutencao);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter manutenções corretivas");
                throw new Exception("Erro ao obter manutenções corretivas: " + ex.Message, ex);
            }

            return lista;
        }

        /// <summary>
        /// Adiciona uma nova manutenção corretiva no banco SQLite.
        /// </summary>
        /// <param name="manutencao">Objeto ManutencaoCorretiva a ser adicionado.</param>
        /// <param name="cancellationToken">Token para cancelamento da operação assíncrona.</param>
        /// <returns>Task representando a operação assíncrona.</returns>
        public async Task AdicionarAsync(ManutencaoCorretiva manutencao, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var connection = await CriarConexaoAsync(cancellationToken))
                {
                    var command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        INSERT INTO ManutencoesCorretivas (VeiculoId, DataRegistro, DescricaoFalha, Observacoes, SolucaoAplicada, FotosComprovantes)
                        VALUES ($veiculoId, $dataRegistro, $descricaoFalha, $observacoes, $solucaoAplicada, $fotosComprovantes);
                    ";

                    command.Parameters.AddWithValue("$veiculoId", manutencao.VeiculoId);
                    command.Parameters.AddWithValue("$dataRegistro", manutencao.DataRegistro);
                    command.Parameters.AddWithValue("$descricaoFalha", manutencao.DescricaoFalha);
                    command.Parameters.AddWithValue("$observacoes", manutencao.Observacoes ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("$solucaoAplicada", manutencao.SolucaoAplicada ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("$fotosComprovantes", manutencao.FotosComprovantes ?? (object)DBNull.Value);

                    await command.ExecuteNonQueryAsync(cancellationToken);
                }

                try
                {
                    await _firebaseHelper.EnviarAsync<ManutencaoCorretiva>(manutencao.Id.ToString(), manutencao, "PUT");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao sincronizar manutenção corretiva com Firebase");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar manutenção corretiva");
                throw new Exception("Erro ao adicionar manutenção corretiva: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Atualiza uma manutenção corretiva existente no banco SQLite.
        /// </summary>
        /// <param name="manutencao">Objeto ManutencaoCorretiva a ser atualizado.</param>
        /// <param name="cancellationToken">Token para cancelamento da operação assíncrona.</param>
        /// <returns>Task representando a operação assíncrona.</returns>
        public async Task AtualizarAsync(ManutencaoCorretiva manutencao, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var connection = await CriarConexaoAsync(cancellationToken))
                {
                    var command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        UPDATE ManutencoesCorretivas
                        SET VeiculoId = $veiculoId,
                            DataRegistro = $dataRegistro,
                            DescricaoFalha = $descricaoFalha,
                            Observacoes = $observacoes,
                            SolucaoAplicada = $solucaoAplicada,
                            FotosComprovantes = $fotosComprovantes
                        WHERE Id = $id;
                    ";

                    command.Parameters.AddWithValue("$veiculoId", manutencao.VeiculoId);
                    command.Parameters.AddWithValue("$dataRegistro", manutencao.DataRegistro);
                    command.Parameters.AddWithValue("$descricaoFalha", manutencao.DescricaoFalha);
                    command.Parameters.AddWithValue("$observacoes", manutencao.Observacoes ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("$solucaoAplicada", manutencao.SolucaoAplicada ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("$fotosComprovantes", manutencao.FotosComprovantes ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("$id", manutencao.Id);

                    await command.ExecuteNonQueryAsync(cancellationToken);
                }

                try
                {
                    await _firebaseHelper.EnviarAsync<ManutencaoCorretiva>(manutencao.Id.ToString(), manutencao, "PUT");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao sincronizar atualização de manutenção corretiva com Firebase");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar manutenção corretiva");
                throw new Exception("Erro ao atualizar manutenção corretiva: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Remove uma manutenção corretiva pelo ID no banco SQLite.
        /// </summary>
        /// <param name="id">ID da manutenção corretiva a ser removida.</param>
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
                        DELETE FROM ManutencoesCorretivas
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
                    _logger.LogError(ex, "Erro ao sincronizar remoção de manutenção corretiva com Firebase");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover manutenção corretiva");
                throw new Exception("Erro ao remover manutenção corretiva: " + ex.Message, ex);
            }
        }
    }
}
