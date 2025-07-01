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
    public class VeiculoDetalhesRepositorio : BaseRepositorio
    {
        private readonly FirebaseHelper _firebaseHelper;
        private readonly IConfiguration _configuration;

        public VeiculoDetalhesRepositorio(IConfiguration configuration, ILogger<VeiculoDetalhesRepositorio>? logger = null)
            : base(logger ?? NullLogger<VeiculoDetalhesRepositorio>.Instance)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _firebaseHelper = new FirebaseHelper(configuration);
        }

        public async Task<List<VeiculoDetalhes>> ObterTodosAsync(CancellationToken cancellationToken = default)
        {
            var lista = new List<VeiculoDetalhes>();

            try
            {
                using (var connection = await CriarConexaoAsync(cancellationToken))
                {
                    var command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        SELECT Id, VeiculoId, Cor, Placa, Foto, Personalizacao, Observacoes
                        FROM VeiculoDetalhes
                        ORDER BY Id ASC;
                    ";

                    using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                    {
                        while (await reader.ReadAsync(cancellationToken))
                        {
                            var detalhes = new VeiculoDetalhes
                            {
                                Id = reader.GetInt32(0),
                                VeiculoId = reader.GetInt32(1),
                                Cor = reader.GetString(2),
                                Placa = reader.GetString(3),
                                Foto = reader.GetString(4),
                                Personalizacao = reader.GetString(5),
                                Observacoes = reader.GetString(6)
                            };
                            lista.Add(detalhes);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter detalhes do veículo");
                throw new Exception("Erro ao obter detalhes do veículo: " + ex.Message, ex);
            }

            return lista;
        }

        public async Task AdicionarAsync(VeiculoDetalhes detalhes, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var connection = await CriarConexaoAsync(cancellationToken))
                {
                    var command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        INSERT INTO VeiculoDetalhes (VeiculoId, Cor, Placa, Foto, Personalizacao, Observacoes)
                        VALUES ($veiculoId, $cor, $placa, $foto, $personalizacao, $observacoes);
                    ";

                    command.Parameters.AddWithValue("$veiculoId", detalhes.VeiculoId);
                    command.Parameters.AddWithValue("$cor", detalhes.Cor);
                    command.Parameters.AddWithValue("$placa", detalhes.Placa);
                    command.Parameters.AddWithValue("$foto", detalhes.Foto);
                    command.Parameters.AddWithValue("$personalizacao", detalhes.Personalizacao);
                    command.Parameters.AddWithValue("$observacoes", detalhes.Observacoes);

                    await command.ExecuteNonQueryAsync(cancellationToken);
                }

                try
                {
                    await _firebaseHelper.EnviarAsync<VeiculoDetalhes>(detalhes.Id.ToString(), detalhes, "PUT");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao sincronizar detalhes do veículo com Firebase");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar detalhes do veículo");
                throw new Exception("Erro ao adicionar detalhes do veículo: " + ex.Message, ex);
            }
        }

        public async Task AtualizarAsync(VeiculoDetalhes detalhes, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var connection = await CriarConexaoAsync(cancellationToken))
                {
                    var command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        UPDATE VeiculoDetalhes
                        SET VeiculoId = $veiculoId,
                            Cor = $cor,
                            Placa = $placa,
                            Foto = $foto,
                            Personalizacao = $personalizacao,
                            Observacoes = $observacoes
                        WHERE Id = $id;
                    ";

                    command.Parameters.AddWithValue("$veiculoId", detalhes.VeiculoId);
                    command.Parameters.AddWithValue("$cor", detalhes.Cor);
                    command.Parameters.AddWithValue("$placa", detalhes.Placa);
                    command.Parameters.AddWithValue("$foto", detalhes.Foto);
                    command.Parameters.AddWithValue("$personalizacao", detalhes.Personalizacao);
                    command.Parameters.AddWithValue("$observacoes", detalhes.Observacoes ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("$id", detalhes.Id);
                    
                    await command.ExecuteNonQueryAsync(cancellationToken);
                }

                try
                {
                    await _firebaseHelper.EnviarAsync<VeiculoDetalhes>(detalhes.Id.ToString(), detalhes, "PUT");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao sincronizar atualização dos detalhes do veículo com Firebase");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar detalhes do veículo");
                throw new Exception("Erro ao atualizar detalhes do veículo: " + ex.Message, ex);
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
                        DELETE FROM VeiculoDetalhes
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
                    _logger.LogError(ex, "Erro ao sincronizar remoção dos detalhes do veículo com Firebase");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover detalhes do veículo");
                throw new Exception("Erro ao remover detalhes do veículo: " + ex.Message, ex);
            }
        }

        public async Task<VeiculoDetalhes?> ObterPorIdAsync(int id)
        {
            try
            {
                using var connection = await CriarConexaoAsync();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM VeiculoDetalhes WHERE Id = @Id";
                command.Parameters.AddWithValue("@Id", id);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new VeiculoDetalhes
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        VeiculoId = reader.GetInt32(reader.GetOrdinal("VeiculoId")),
                        Quilometragem = reader.GetDecimal(reader.GetOrdinal("Quilometragem")),
                        DataAtualizacao = reader.GetDateTime(reader.GetOrdinal("DataAtualizacao")),
                        Cor = reader.IsDBNull(reader.GetOrdinal("Cor")) ? string.Empty : reader.GetString(reader.GetOrdinal("Cor")),
                        Placa = reader.IsDBNull(reader.GetOrdinal("Placa")) ? string.Empty : reader.GetString(reader.GetOrdinal("Placa")),
                        Foto = reader.IsDBNull(reader.GetOrdinal("Foto")) ? string.Empty : reader.GetString(reader.GetOrdinal("Foto")),
                        Personalizacao = reader.IsDBNull(reader.GetOrdinal("Personalizacao")) ? string.Empty : reader.GetString(reader.GetOrdinal("Personalizacao")),
                        Observacoes = reader.IsDBNull(reader.GetOrdinal("Observacoes")) ? string.Empty : reader.GetString(reader.GetOrdinal("Observacoes"))
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter detalhes do veículo por ID: {Id}", id);
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
