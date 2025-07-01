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
    public class ControlePneusRepositorio : BaseRepositorio
    {
        private readonly FirebaseHelper _firebaseHelper;
        private readonly IConfiguration _configuration;

        public ControlePneusRepositorio(IConfiguration configuration, ILogger<ControlePneusRepositorio>? logger = null)
            : base(logger ?? NullLogger<ControlePneusRepositorio>.Instance)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _firebaseHelper = new FirebaseHelper(configuration);
        }

        /// <summary>
        /// Obtém todos os controles de pneus do banco SQLite.
        /// </summary>
        /// <param name="cancellationToken">Token para cancelamento da operação assíncrona.</param>
        /// <returns>Lista de objetos ControlePneus.</returns>
        public async Task<List<ControlePneus>> ObterTodosAsync(CancellationToken cancellationToken = default)
        {
            var lista = new List<ControlePneus>();

            try
            {
                using (var connection = await CriarConexaoAsync(cancellationToken))
                {
                    var command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        SELECT Id, VeiculoId, Posicao, DataInstalacao, QuilometragemInstalacao, DataRemocao, QuilometragemRemocao, Observacoes
                        FROM ControlePneus
                        ORDER BY DataInstalacao ASC;
                    ";

                    using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                    {
                        while (await reader.ReadAsync(cancellationToken))
                        {
                            var pneu = new ControlePneus
                            {
                            Id = reader.GetInt32(0),
                            VeiculoId = reader.GetInt32(1),
                            Posicao = reader.GetString(2),
                            DataInstalacao = reader.GetDateTime(3),
                            QuilometragemInstalacao = reader.GetInt32(4),
                            DataRemocao = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5),
                            QuilometragemRemocao = reader.IsDBNull(6) ? (int?)null : reader.GetInt32(6),
                            Observacoes = reader.IsDBNull(7) ? default! : reader.GetString(7)
                        };
                            lista.Add(pneu);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter controle de pneus");
                throw new Exception("Erro ao obter controle de pneus: " + ex.Message, ex);
            }

            return lista;
        }

        public async Task AdicionarAsync(ControlePneus pneu, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var connection = await CriarConexaoAsync(cancellationToken))
                {
                    var command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        INSERT INTO ControlePneus (VeiculoId, Posicao, DataInstalacao, QuilometragemInstalacao, DataRemocao, QuilometragemRemocao, Observacoes)
                        VALUES ($veiculoId, $posicao, $dataInstalacao, $quilometragemInstalacao, $dataRemocao, $quilometragemRemocao, $observacoes);
                    ";

                    command.Parameters.AddWithValue("$veiculoId", pneu.VeiculoId);
                    command.Parameters.AddWithValue("$posicao", pneu.Posicao);
                    command.Parameters.AddWithValue("$dataInstalacao", pneu.DataInstalacao);
                    command.Parameters.AddWithValue("$quilometragemInstalacao", pneu.QuilometragemInstalacao);
                    command.Parameters.AddWithValue("$dataRemocao", pneu.DataRemocao ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("$quilometragemRemocao", pneu.QuilometragemRemocao ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("$observacoes", pneu.Observacoes ?? (object)DBNull.Value);

                    await command.ExecuteNonQueryAsync(cancellationToken);
                }

                try
                {
                    await _firebaseHelper.EnviarAsync<ControlePneus>(pneu.Id.ToString(), pneu, "PUT");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao sincronizar controle de pneus com Firebase");
                    // Decidir se deve lançar ou apenas logar
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar controle de pneus");
                throw new Exception("Erro ao adicionar controle de pneus: " + ex.Message, ex);
            }
        }

        public async Task AtualizarAsync(ControlePneus pneu, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var connection = await CriarConexaoAsync(cancellationToken))
                {
                    var command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        UPDATE ControlePneus
                        SET VeiculoId = $veiculoId,
                            Posicao = $posicao,
                            DataInstalacao = $dataInstalacao,
                            QuilometragemInstalacao = $quilometragemInstalacao,
                            DataRemocao = $dataRemocao,
                            QuilometragemRemocao = $quilometragemRemocao,
                            Observacoes = $observacoes
                        WHERE Id = $id;
                    ";

                    command.Parameters.AddWithValue("$veiculoId", pneu.VeiculoId);
                    command.Parameters.AddWithValue("$posicao", pneu.Posicao);
                    command.Parameters.AddWithValue("$dataInstalacao", pneu.DataInstalacao);
                    command.Parameters.AddWithValue("$quilometragemInstalacao", pneu.QuilometragemInstalacao);
                    command.Parameters.AddWithValue("$dataRemocao", pneu.DataRemocao ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("$quilometragemRemocao", pneu.QuilometragemRemocao ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("$observacoes", pneu.Observacoes ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("$id", pneu.Id);

                    await command.ExecuteNonQueryAsync(cancellationToken);
                }

                try
                {
                    await _firebaseHelper.EnviarAsync<ControlePneus>(pneu.Id.ToString(), pneu, "PUT");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao sincronizar atualização de controle de pneus com Firebase");
                    // Decidir se deve lançar ou apenas logar
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar controle de pneus");
                throw new Exception("Erro ao atualizar controle de pneus: " + ex.Message, ex);
            }
        }

        public async Task RemoverAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var connection = await CriarConexaoAsync(cancellationToken))
                {
                    var command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        DELETE FROM ControlePneus
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
                    _logger.LogError(ex, "Erro ao sincronizar remoção de controle de pneus com Firebase");
                    // Decidir se deve lançar ou apenas logar
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover controle de pneus");
                throw new Exception("Erro ao remover controle de pneus: " + ex.Message, ex);
            }
        }
    }
}
