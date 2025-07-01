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
    public class UsuarioRepositorio : BaseRepositorio
    {
        private readonly FirebaseHelper _firebaseHelper;
        private readonly IConfiguration _configuration;

        public UsuarioRepositorio(IConfiguration configuration, ILogger<UsuarioRepositorio>? logger = null)
            : base(logger ?? NullLogger<UsuarioRepositorio>.Instance)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _firebaseHelper = new FirebaseHelper(configuration);
        }

        /// <summary>
        /// Inicializa o banco de dados criando as tabelas necessárias.
        /// </summary>
        public async Task InicializarBancoAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var assembly = typeof(UsuarioRepositorio).Assembly;

                string LerRecurso(string nomeRecurso)
                {
                    using var stream = assembly.GetManifestResourceStream(nomeRecurso);
                    if (stream == null)
                    {
                        throw new Exception($"Recurso incorporado não encontrado: {nomeRecurso}");
                    }
                    using var reader = new System.IO.StreamReader(stream);
                    return reader.ReadToEnd();
                }

                // Ajuste os nomes dos recursos conforme o namespace e pasta do projeto
                var scriptUsuarios = LerRecurso("VeiculoCertinho.Database.CreateUsuariosTable.sql");
                var scriptVeiculo = LerRecurso("VeiculoCertinho.Database.CreateVeiculoTable.sql");
                var scriptVeiculoDetalhes = LerRecurso("VeiculoCertinho.Database.CreateVeiculoDetalhesTable.sql");

                _logger.LogInformation("Executando script de criação da tabela Usuarios");
                await ExecutarScriptSqlAsync(scriptUsuarios, cancellationToken);

                _logger.LogInformation("Executando script de criação da tabela Veiculo");
                await ExecutarScriptSqlAsync(scriptVeiculo, cancellationToken);

                _logger.LogInformation("Executando script de criação da tabela VeiculoDetalhes");
                await ExecutarScriptSqlAsync(scriptVeiculoDetalhes, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao inicializar o banco de dados");
                throw;
            }
        }

        public async Task<List<Usuario>> ObterTodosAsync(CancellationToken cancellationToken = default)
        {
            var lista = new List<Usuario>();

            try
            {
                using (var connection = await CriarConexaoAsync(cancellationToken))
                {
                    var command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        SELECT Id, Nome, Email, SenhaHash, DoisFatoresAtivado, Perfil, DataCriacao, UltimoLogin
                        FROM Usuarios
                        ORDER BY Nome ASC;
                    ";

                    using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                    {
                        while (await reader.ReadAsync(cancellationToken))
                        {
                            var usuario = new Usuario
                            {
                                Id = reader.GetInt32(0),
                                Nome = reader.GetString(1),
                                Email = reader.GetString(2),
                                SenhaHash = reader.GetString(3),
                                DoisFatoresAtivado = reader.GetBoolean(4),
                                Perfil = reader.GetString(5),
                                UltimoLogin = reader.IsDBNull(7) ? default(DateTime) : reader.GetDateTime(7)
                            };
                            lista.Add(usuario);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter usuarios");
                throw new Exception("Erro ao obter usuarios: " + ex.Message, ex);
            }

            return lista;
        }

        public async Task AdicionarAsync(Usuario usuario, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var connection = await CriarConexaoAsync(cancellationToken))
                {
                    var command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        INSERT INTO Usuarios (Nome, Email, SenhaHash, DoisFatoresAtivado, Perfil, DataCriacao, UltimoLogin)
                        VALUES ($nome, $email, $senhaHash, $doisFatoresAtivado, $perfil, $dataCriacao, $ultimoLogin);
                    ";

                    command.Parameters.AddWithValue("$nome", usuario.Nome);
                    command.Parameters.AddWithValue("$email", usuario.Email);
                    command.Parameters.AddWithValue("$senhaHash", usuario.SenhaHash);
                    command.Parameters.AddWithValue("$doisFatoresAtivado", usuario.DoisFatoresAtivado);
                    command.Parameters.AddWithValue("$perfil", usuario.Perfil);
                    command.Parameters.AddWithValue("$dataCriacao", usuario.DataCriacao);
                    command.Parameters.AddWithValue("$ultimoLogin", usuario.UltimoLogin);

                    await command.ExecuteNonQueryAsync(cancellationToken);
                }

                try
                {
                    await _firebaseHelper.EnviarAsync<Usuario>(usuario.Id.ToString(), usuario, "PUT");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao sincronizar usuario com Firebase");
                    // Decidir se deve lançar ou apenas logar
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar usuario");
                throw new Exception("Erro ao adicionar usuario: " + ex.Message, ex);
            }
        }

        public async Task AtualizarAsync(Usuario usuario, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var connection = await CriarConexaoAsync(cancellationToken))
                {
                    var command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        UPDATE Usuarios
                        SET Nome = $nome,
                            Email = $email,
                            SenhaHash = $senhaHash,
                            DoisFatoresAtivado = $doisFatoresAtivado,
                            Perfil = $perfil,
                            DataCriacao = $dataCriacao,
                            UltimoLogin = $ultimoLogin
                        WHERE Id = $id;
                    ";

                    command.Parameters.AddWithValue("$nome", usuario.Nome);
                    command.Parameters.AddWithValue("$email", usuario.Email);
                    command.Parameters.AddWithValue("$senhaHash", usuario.SenhaHash);
                    command.Parameters.AddWithValue("$doisFatoresAtivado", usuario.DoisFatoresAtivado);
                    command.Parameters.AddWithValue("$perfil", usuario.Perfil);
                    command.Parameters.AddWithValue("$dataCriacao", usuario.DataCriacao);
                    command.Parameters.AddWithValue("$ultimoLogin", usuario.UltimoLogin);
                    command.Parameters.AddWithValue("$id", usuario.Id);

                    await command.ExecuteNonQueryAsync(cancellationToken);
                }

                try
                {
                    await _firebaseHelper.EnviarAsync<Usuario>(usuario.Id.ToString(), usuario, "PUT");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao sincronizar atualização de usuario com Firebase");
                    // Decidir se deve lançar ou apenas logar
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar usuario");
                throw new Exception("Erro ao atualizar usuario: " + ex.Message, ex);
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
                        DELETE FROM Usuarios
                        WHERE Id = $id;
                    ";

                    command.Parameters.AddWithValue("$id", id);

                    await command.ExecuteNonQueryAsync(cancellationToken);
                }

                try
                {
                    await _firebaseHelper.EnviarAsync<object>(id.ToString(), new object(), "DELETE");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao sincronizar remoção de usuario com Firebase");
                    // Decidir se deve lançar ou apenas logar
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover usuario");
                throw new Exception("Erro ao remover usuario: " + ex.Message, ex);
            }
        }

        public async Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var connection = await CriarConexaoAsync(cancellationToken))
                {
                    var command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        SELECT Id, Nome, Email, SenhaHash, DoisFatoresAtivado, Perfil, DataCriacao, UltimoLogin
                        FROM Usuarios
                        WHERE Email = $email;
                    ";

                    command.Parameters.AddWithValue("$email", email);

                    using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                    {
                        if (await reader.ReadAsync(cancellationToken))
                        {
                            return new Usuario
                            {
                                Id = reader.GetInt32(0),
                                Nome = reader.GetString(1),
                                Email = reader.GetString(2),
                                SenhaHash = reader.GetString(3),
                                DoisFatoresAtivado = reader.GetBoolean(4),
                                Perfil = reader.GetString(5),
                                UltimoLogin = reader.IsDBNull(7) ? default(DateTime) : reader.GetDateTime(7)
                            };
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter usuário por email: {Email}", email);
                throw;
            }
        }
    }
}
