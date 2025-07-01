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
    public class CompartilhamentoRepositorio : BaseRepositorio
    {
        private readonly FirebaseHelper _firebaseHelper;
        private readonly IConfiguration _configuration;

        public CompartilhamentoRepositorio(IConfiguration configuration, ILogger<CompartilhamentoRepositorio>? logger = null)
            : base(logger ?? NullLogger<CompartilhamentoRepositorio>.Instance)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _firebaseHelper = new FirebaseHelper(configuration);
        }

        /// <summary>
        /// Obtém todos os compartilhamentos do banco SQLite.
        /// </summary>
        /// <param name="cancellationToken">Token para cancelamento da operação assíncrona.</param>
        /// <returns>Lista de objetos Compartilhamento.</returns>
        public async Task<List<Compartilhamento>> ObterTodosAsync(CancellationToken cancellationToken = default)
        {
            var lista = new List<Compartilhamento>();

            try
            {
                using (var connection = await CriarConexaoAsync(cancellationToken))
                {
                    var command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        SELECT Id, VeiculoId, UsuarioId, Permissoes, DataCompartilhamento, Ativo
                        FROM Compartilhamentos
                        ORDER BY DataCompartilhamento DESC;
                    ";

                    using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                    {
                        while (await reader.ReadAsync(cancellationToken))
                        {
                            var compartilhamento = new Compartilhamento
                            {
                                Id = reader.GetInt32(0),
                                VeiculoId = reader.GetInt32(1),
                                UsuarioId = reader.GetInt32(2),
                                Permissoes = reader.GetString(3),
                                Ativo = reader.GetBoolean(5)
                            };
                            lista.Add(compartilhamento);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter compartilhamentos");
                throw new Exception("Erro ao obter compartilhamentos: " + ex.Message, ex);
            }

            return lista;
        }

        public async Task AdicionarAsync(Compartilhamento compartilhamento, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var connection = await CriarConexaoAsync(cancellationToken))
                {
                    var command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        INSERT INTO Compartilhamentos (VeiculoId, UsuarioId, Permissoes, DataCompartilhamento, Ativo)
                        VALUES ($veiculoId, $usuarioId, $permissoes, $dataCompartilhamento, $ativo);
                    ";

                    command.Parameters.AddWithValue("$veiculoId", compartilhamento.VeiculoId);
                    command.Parameters.AddWithValue("$usuarioId", compartilhamento.UsuarioId);
                    command.Parameters.AddWithValue("$permissoes", compartilhamento.Permissoes);
                    command.Parameters.AddWithValue("$dataCompartilhamento", compartilhamento.DataCompartilhamento);
                    command.Parameters.AddWithValue("$ativo", compartilhamento.Ativo);

                    await command.ExecuteNonQueryAsync(cancellationToken);
                }

                try
                {
                    await _firebaseHelper.EnviarAsync<Compartilhamento>(compartilhamento.Id.ToString(), compartilhamento, "PUT");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao sincronizar compartilhamento com Firebase");
                    // Decidir se deve lançar ou apenas logar
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar compartilhamento");
                throw new Exception("Erro ao adicionar compartilhamento: " + ex.Message, ex);
            }
        }

        public async Task AtualizarAsync(Compartilhamento compartilhamento, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var connection = await CriarConexaoAsync(cancellationToken))
                {
                    var command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        UPDATE Compartilhamentos
                        SET VeiculoId = $veiculoId,
                            UsuarioId = $usuarioId,
                            Permissoes = $permissoes,
                            DataCompartilhamento = $dataCompartilhamento,
                            Ativo = $ativo
                        WHERE Id = $id;
                    ";

                    command.Parameters.AddWithValue("$veiculoId", compartilhamento.VeiculoId);
                    command.Parameters.AddWithValue("$usuarioId", compartilhamento.UsuarioId);
                    command.Parameters.AddWithValue("$permissoes", compartilhamento.Permissoes);
                    command.Parameters.AddWithValue("$dataCompartilhamento", compartilhamento.DataCompartilhamento);
                    command.Parameters.AddWithValue("$ativo", compartilhamento.Ativo);
                    command.Parameters.AddWithValue("$id", compartilhamento.Id);

                    await command.ExecuteNonQueryAsync(cancellationToken);
                }

                try
                {
                    await _firebaseHelper.EnviarAsync<Compartilhamento>(compartilhamento.Id.ToString(), compartilhamento, "PUT");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao sincronizar atualização de compartilhamento com Firebase");
                    // Decidir se deve lançar ou apenas logar
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar compartilhamento");
                throw new Exception("Erro ao atualizar compartilhamento: " + ex.Message, ex);
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
                        DELETE FROM Compartilhamentos
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
                    _logger.LogError(ex, "Erro ao sincronizar remoção de compartilhamento com Firebase");
                    // Decidir se deve lançar ou apenas logar
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover compartilhamento");
                throw new Exception("Erro ao remover compartilhamento: " + ex.Message, ex);
            }
        }
    }
}
