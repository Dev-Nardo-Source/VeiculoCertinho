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
    public class RelatorioRepositorio : BaseRepositorio
    {
        private readonly FirebaseHelper _firebaseHelper;
        private readonly IConfiguration _configuration;

        public RelatorioRepositorio(IConfiguration configuration, ILogger<RelatorioRepositorio>? logger = null)
            : base(logger ?? NullLogger<RelatorioRepositorio>.Instance)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _firebaseHelper = new FirebaseHelper(configuration);
        }

        /// <summary>
        /// Obtém todos os relatórios do banco SQLite.
        /// </summary>
        /// <param name="cancellationToken">Token para cancelamento da operação assíncrona.</param>
        /// <returns>Lista de objetos Relatorio.</returns>
        public async Task<List<Relatorio>> ObterTodosAsync(CancellationToken cancellationToken = default)
        {
            var lista = new List<Relatorio>();

            try
            {
                using (var connection = await CriarConexaoAsync(cancellationToken))
                {
                    var command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        SELECT Id, VeiculoId, TipoRelatorio, DataGeracao, CaminhoArquivo, Observacoes
                        FROM Relatorios
                        ORDER BY DataGeracao DESC;
                    ";

                    using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                    {
                        while (await reader.ReadAsync(cancellationToken))
                        {
                            var relatorio = new Relatorio
                            {
                                Id = reader.GetInt32(0),
                                VeiculoId = reader.GetInt32(1),
                                TipoRelatorio = reader.GetString(2),
                                DataGeracao = reader.GetDateTime(3),
                                CaminhoArquivo = reader.GetString(4),
                                Observacoes = reader.IsDBNull(5) ? default! : reader.GetString(5)
                            };
                            lista.Add(relatorio);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter relatórios");
                throw new Exception("Erro ao obter relatórios: " + ex.Message, ex);
            }

            return lista;
        }

        /// <summary>
        /// Adiciona um novo relatório no banco SQLite.
        /// </summary>
        /// <param name="relatorio">Objeto Relatorio a ser adicionado.</param>
        /// <param name="cancellationToken">Token para cancelamento da operação assíncrona.</param>
        /// <returns>Task representando a operação assíncrona.</returns>
        public async Task AdicionarAsync(Relatorio relatorio, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var connection = await CriarConexaoAsync(cancellationToken))
                {
                    var command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        INSERT INTO Relatorios (VeiculoId, TipoRelatorio, DataGeracao, CaminhoArquivo, Observacoes)
                        VALUES ($veiculoId, $tipoRelatorio, $dataGeracao, $caminhoArquivo, $observacoes);
                    ";

                    command.Parameters.AddWithValue("$veiculoId", relatorio.VeiculoId);
                    command.Parameters.AddWithValue("$tipoRelatorio", relatorio.TipoRelatorio);
                    command.Parameters.AddWithValue("$dataGeracao", relatorio.DataGeracao);
                    command.Parameters.AddWithValue("$caminhoArquivo", relatorio.CaminhoArquivo);
                    command.Parameters.AddWithValue("$observacoes", relatorio.Observacoes ?? (object)DBNull.Value);

                    await command.ExecuteNonQueryAsync(cancellationToken);
                }

                try
                {
                    await _firebaseHelper.EnviarAsync<Relatorio>(relatorio.Id.ToString(), relatorio, "PUT");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao sincronizar relatório com Firebase");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar relatório");
                throw new Exception("Erro ao adicionar relatório: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Atualiza um relatório existente no banco SQLite.
        /// </summary>
        /// <param name="relatorio">Objeto Relatorio a ser atualizado.</param>
        /// <param name="cancellationToken">Token para cancelamento da operação assíncrona.</param>
        /// <returns>Task representando a operação assíncrona.</returns>
        public async Task AtualizarAsync(Relatorio relatorio, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var connection = await CriarConexaoAsync(cancellationToken))
                {
                    var command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        UPDATE Relatorios
                        SET VeiculoId = $veiculoId,
                            TipoRelatorio = $tipoRelatorio,
                            DataGeracao = $dataGeracao,
                            CaminhoArquivo = $caminhoArquivo,
                            Observacoes = $observacoes
                        WHERE Id = $id;
                    ";

                    command.Parameters.AddWithValue("$veiculoId", relatorio.VeiculoId);
                    command.Parameters.AddWithValue("$tipoRelatorio", relatorio.TipoRelatorio);
                    command.Parameters.AddWithValue("$dataGeracao", relatorio.DataGeracao);
                    command.Parameters.AddWithValue("$caminhoArquivo", relatorio.CaminhoArquivo);
                    command.Parameters.AddWithValue("$observacoes", relatorio.Observacoes ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("$id", relatorio.Id);

                    await command.ExecuteNonQueryAsync(cancellationToken);
                }

                try
                {
                    await _firebaseHelper.EnviarAsync<Relatorio>(relatorio.Id.ToString(), relatorio, "PUT");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao sincronizar atualização de relatório com Firebase");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar relatório");
                throw new Exception("Erro ao atualizar relatório: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Remove um relatório pelo ID no banco SQLite.
        /// </summary>
        /// <param name="id">ID do relatório a ser removido.</param>
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
                        DELETE FROM Relatorios
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
                    _logger.LogError(ex, "Erro ao sincronizar remoção de relatório com Firebase");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover relatório");
                throw new Exception("Erro ao remover relatório: " + ex.Message, ex);
            }
        }
    }
}
