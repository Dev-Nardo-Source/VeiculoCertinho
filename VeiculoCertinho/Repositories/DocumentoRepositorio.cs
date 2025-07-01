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
using System.Data;

namespace VeiculoCertinho.Repositories
{
    public class DocumentoRepositorio : BaseRepositorio
    {
        private readonly FirebaseHelper _firebaseHelper;
        private readonly IConfiguration _configuration;

        public DocumentoRepositorio(IConfiguration configuration, ILogger<DocumentoRepositorio>? logger = null)
            : base(logger ?? NullLogger<DocumentoRepositorio>.Instance)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _firebaseHelper = new FirebaseHelper(configuration);
        }

        public async Task<List<Documento>> ObterTodosAsync(CancellationToken cancellationToken = default)
        {
            const string query = @"
                SELECT Id, VeiculoId, TipoDocumento, DataVencimento, Notificado, Observacoes
                FROM Documentos
                ORDER BY DataVencimento ASC";

            return await ExecuteQueryAsync(query, MapDocumento, cancellationToken: cancellationToken);
        }

        public async Task<Documento?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
        {
            const string query = @"
                SELECT Id, VeiculoId, TipoDocumento, DataVencimento, Notificado, Observacoes
                FROM Documentos
                WHERE Id = $id";

            var parameters = new Dictionary<string, object> { { "id", id } };
            return await ExecuteQuerySingleAsync(query, MapDocumento, parameters, cancellationToken);
        }

        public async Task<List<Documento>> ObterPorVeiculoIdAsync(int veiculoId, CancellationToken cancellationToken = default)
        {
            const string query = @"
                SELECT Id, VeiculoId, TipoDocumento, DataVencimento, Notificado, Observacoes
                FROM Documentos
                WHERE VeiculoId = $veiculoId
                ORDER BY DataVencimento ASC";

            var parameters = new Dictionary<string, object> { { "veiculoId", veiculoId } };
            return await ExecuteQueryAsync(query, MapDocumento, parameters, cancellationToken);
        }

        public async Task<List<Documento>> ObterVencidosAsync(CancellationToken cancellationToken = default)
        {
            const string query = @"
                SELECT Id, VeiculoId, TipoDocumento, DataVencimento, Notificado, Observacoes
                FROM Documentos
                WHERE DataVencimento < DATE('now')
                ORDER BY DataVencimento ASC";

            return await ExecuteQueryAsync(query, MapDocumento, cancellationToken: cancellationToken);
        }

        public async Task<List<Documento>> ObterProximosDoVencimentoAsync(int dias = 30, CancellationToken cancellationToken = default)
        {
            const string query = @"
                SELECT Id, VeiculoId, TipoDocumento, DataVencimento, Notificado, Observacoes
                FROM Documentos
                WHERE DataVencimento BETWEEN DATE('now') AND DATE('now', '+{0} days')
                ORDER BY DataVencimento ASC";

            var formattedQuery = string.Format(query, dias);
            return await ExecuteQueryAsync(formattedQuery, MapDocumento, cancellationToken: cancellationToken);
        }

        public async Task AdicionarAsync(Documento documento, CancellationToken cancellationToken = default)
        {
            const string command = @"
                INSERT INTO Documentos (VeiculoId, TipoDocumento, DataVencimento, Notificado, Observacoes)
                VALUES ($veiculoId, $tipoDocumento, $dataVencimento, $notificado, $observacoes)";

            var parameters = new Dictionary<string, object>
            {
                { "veiculoId", documento.VeiculoId },
                { "tipoDocumento", documento.TipoDocumento },
                { "dataVencimento", documento.DataVencimento },
                { "notificado", documento.Notificado },
                { "observacoes", documento.Observacoes ?? (object)DBNull.Value }
            };

            await ExecuteCommandAsync(command, parameters, cancellationToken);

            // Sincronização com Firebase (opcional, não falha a operação principal)
            try
            {
                await _firebaseHelper.EnviarAsync<Documento>(documento.Id.ToString(), documento, "PUT");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao sincronizar documento com Firebase (ID: {Id})", documento.Id);
            }
        }

        public async Task AtualizarAsync(Documento documento, CancellationToken cancellationToken = default)
        {
            const string command = @"
                UPDATE Documentos
                SET VeiculoId = $veiculoId,
                    TipoDocumento = $tipoDocumento,
                    DataVencimento = $dataVencimento,
                    Notificado = $notificado,
                    Observacoes = $observacoes
                WHERE Id = $id";

            var parameters = new Dictionary<string, object>
            {
                { "veiculoId", documento.VeiculoId },
                { "tipoDocumento", documento.TipoDocumento },
                { "dataVencimento", documento.DataVencimento },
                { "notificado", documento.Notificado },
                { "observacoes", documento.Observacoes ?? (object)DBNull.Value },
                { "id", documento.Id }
            };

            await ExecuteCommandAsync(command, parameters, cancellationToken);

            // Sincronização com Firebase (opcional)
            try
            {
                await _firebaseHelper.EnviarAsync<Documento>(documento.Id.ToString(), documento, "PUT");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao sincronizar atualização do documento com Firebase (ID: {Id})", documento.Id);
            }
        }

        public async Task RemoverAsync(int id, CancellationToken cancellationToken = default)
        {
            const string command = "DELETE FROM Documentos WHERE Id = $id";
            var parameters = new Dictionary<string, object> { { "id", id } };

            await ExecuteCommandAsync(command, parameters, cancellationToken);

            // Sincronização com Firebase (opcional)
            try
            {
                await _firebaseHelper.EnviarAsync<object>(id.ToString(), default!, "DELETE");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao sincronizar remoção do documento com Firebase (ID: {Id})", id);
            }
        }

        public async Task MarcarComoNotificadoAsync(int id, CancellationToken cancellationToken = default)
        {
            const string command = "UPDATE Documentos SET Notificado = 1 WHERE Id = $id";
            var parameters = new Dictionary<string, object> { { "id", id } };
            await ExecuteCommandAsync(command, parameters, cancellationToken);
        }

        public async Task<int> ContarDocumentosVencidosAsync(int veiculoId, CancellationToken cancellationToken = default)
        {
            const string query = @"
                SELECT COUNT(*) 
                FROM Documentos 
                WHERE VeiculoId = $veiculoId AND DataVencimento < DATE('now')";

            var parameters = new Dictionary<string, object> { { "veiculoId", veiculoId } };
            var resultado = await ExecuteQuerySingleStructAsync(query, reader => GetSafeValue<int>(reader, 0), parameters, cancellationToken);
            return resultado ?? 0;
        }

        public async Task<int> ContarDocumentosAsync(CancellationToken cancellationToken = default)
        {
            const string query = "SELECT COUNT(*) FROM Documentos";
            
            var resultado = await ExecuteQuerySingleStructAsync<int>(query, reader => reader.GetInt32(0), null, cancellationToken);
            return resultado ?? 0;
        }

        // Método de mapeamento privado
        private static Documento MapDocumento(IDataReader reader)
        {
            return new Documento
            {
                Id = GetSafeValue<int>(reader, 0),
                VeiculoId = GetSafeValue<int>(reader, 1),
                TipoDocumento = GetSafeValue<string>(reader, 2, string.Empty),
                DataVencimento = GetSafeValue<DateTime>(reader, 3),
                Notificado = GetSafeValue<bool>(reader, 4),
                Observacoes = GetSafeValue<string>(reader, 5, string.Empty)
            };
        }
    }
}
