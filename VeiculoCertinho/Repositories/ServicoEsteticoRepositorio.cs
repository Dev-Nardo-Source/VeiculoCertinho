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
    public class ServicoEsteticoRepositorio : BaseRepositorio
    {
        private readonly FirebaseHelper _firebaseHelper;
        private readonly IConfiguration _configuration;

        public ServicoEsteticoRepositorio(IConfiguration configuration, ILogger<ServicoEsteticoRepositorio>? logger = null)
            : base(logger ?? NullLogger<ServicoEsteticoRepositorio>.Instance)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _firebaseHelper = new FirebaseHelper(configuration);
        }

        public async Task<List<ServicoEstetico>> ObterTodosAsync(CancellationToken cancellationToken = default)
        {
            var lista = new List<ServicoEstetico>();

            try
            {
                using (var connection = await CriarConexaoAsync(cancellationToken))
                {
                    var command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        SELECT Id, VeiculoId, TipoServico, DataServico, Custo, Observacoes
                        FROM ServicosEsteticos
                        ORDER BY DataServico ASC;
                    ";

                    using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                    {
                        while (await reader.ReadAsync(cancellationToken))
                        {
                            var servico = new ServicoEstetico
                            {
                                Id = reader.GetInt32(0),
                                VeiculoId = reader.GetInt32(1),
                                TipoServico = reader.GetString(2),
                                DataServico = reader.GetDateTime(3),
                                Custo = reader.GetDecimal(4),
                                Observacoes = reader.IsDBNull(5) ? string.Empty : reader.GetString(5)
                            };
                            lista.Add(servico);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter serviços estéticos");
                throw new Exception("Erro ao obter serviços estéticos: " + ex.Message, ex);
            }

            return lista;
        }

        public async Task AdicionarAsync(ServicoEstetico servico, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var connection = await CriarConexaoAsync(cancellationToken))
                {
                    var command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        INSERT INTO ServicosEsteticos (VeiculoId, TipoServico, DataServico, Custo, Observacoes)
                        VALUES ($veiculoId, $tipoServico, $dataServico, $custo, $observacoes);
                    ";

                    command.Parameters.AddWithValue("$veiculoId", servico.VeiculoId);
                    command.Parameters.AddWithValue("$tipoServico", servico.TipoServico);
                    command.Parameters.AddWithValue("$dataServico", servico.DataServico);
                    command.Parameters.AddWithValue("$custo", servico.Custo);
                    command.Parameters.AddWithValue("$observacoes", servico.Observacoes ?? (object)DBNull.Value);

                    await command.ExecuteNonQueryAsync(cancellationToken);
                }

                try
                {
                    await _firebaseHelper.EnviarAsync<ServicoEstetico>(servico.Id.ToString(), servico, "PUT");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao sincronizar serviço estético com Firebase");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar serviço estético");
                throw new Exception("Erro ao adicionar serviço estético: " + ex.Message, ex);
            }
        }

        public async Task AtualizarAsync(ServicoEstetico servico, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var connection = await CriarConexaoAsync(cancellationToken))
                {
                    var command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        UPDATE ServicosEsteticos
                        SET VeiculoId = $veiculoId,
                            TipoServico = $tipoServico,
                            DataServico = $dataServico,
                            Custo = $custo,
                            Observacoes = $observacoes
                        WHERE Id = $id;
                    ";

                    command.Parameters.AddWithValue("$veiculoId", servico.VeiculoId);
                    command.Parameters.AddWithValue("$tipoServico", servico.TipoServico);
                    command.Parameters.AddWithValue("$dataServico", servico.DataServico);
                    command.Parameters.AddWithValue("$custo", servico.Custo);
                    command.Parameters.AddWithValue("$observacoes", servico.Observacoes ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("$id", servico.Id);

                    await command.ExecuteNonQueryAsync(cancellationToken);
                }

                try
                {
                    await _firebaseHelper.EnviarAsync<ServicoEstetico>(servico.Id.ToString(), servico, "PUT");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao sincronizar atualização de serviço estético com Firebase");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar serviço estético");
                throw new Exception("Erro ao atualizar serviço estético: " + ex.Message, ex);
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
                        DELETE FROM ServicosEsteticos
                        WHERE Id = $id;
                    ";

                    command.Parameters.AddWithValue("$id", id);

                    await command.ExecuteNonQueryAsync(cancellationToken);
                }

                try
                {
                    await _firebaseHelper.EnviarAsync(id.ToString(), new DeletedObject(), "DELETE");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao sincronizar remoção de serviço estético com Firebase");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover serviço estético");
                throw new Exception("Erro ao remover serviço estético: " + ex.Message, ex);
            }
        }

        public async Task<ServicoEstetico?> ObterPorIdAsync(int id)
        {
            try
            {
                using var connection = await CriarConexaoAsync();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM ServicoEstetico WHERE Id = @Id";
                command.Parameters.AddWithValue("@Id", id);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new ServicoEstetico
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        VeiculoId = reader.GetInt32(reader.GetOrdinal("VeiculoId")),
                        DataServico = reader.GetDateTime(reader.GetOrdinal("DataServico")),
                        TipoServico = reader.GetString(reader.GetOrdinal("TipoServico")),
                        Descricao = reader.IsDBNull(reader.GetOrdinal("Descricao")) ? string.Empty : reader.GetString(reader.GetOrdinal("Descricao")),
                        Valor = reader.GetDecimal(reader.GetOrdinal("Valor")),
                        Observacoes = reader.IsDBNull(reader.GetOrdinal("Observacoes")) ? string.Empty : reader.GetString(reader.GetOrdinal("Observacoes"))
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter serviço estético por ID: {Id}", id);
                throw;
            }
        }

        public async Task EnviarAsync<T>(string caminho, T? objeto, string metodo)
        {
            try
            {
                await _firebaseHelper.EnviarAsync(caminho, objeto, metodo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar dados para Firebase: {Caminho}", caminho);
                throw;
            }
        }
    }
}
