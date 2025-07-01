using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VeiculoCertinho.Models;
using VeiculoCertinho.Repositories;
using Microsoft.Extensions.Configuration;

namespace VeiculoCertinho.Services
{
    /// <summary>
    /// Serviço para operações com documentos de veículos.
    /// Agora usa GenericService para eliminar código boilerplate.
    /// </summary>
    public class DocumentoService : GenericService<Documento, DocumentoRepositorio>
    {
        public DocumentoService(IConfiguration configuration, ILogger<DocumentoService> logger)
            : base(new DocumentoRepositorio(configuration, logger), logger)
        {
        }

        #region Implementação dos métodos abstratos do GenericService

        protected override async Task<List<Documento>> ObterTodosFromRepositoryAsync(CancellationToken cancellationToken)
        {
            return await _repository.ObterTodosAsync(cancellationToken);
        }

        protected override async Task<Documento?> ObterPorIdFromRepositoryAsync(int id, CancellationToken cancellationToken)
        {
            return await _repository.ObterPorIdAsync(id, cancellationToken);
        }

        protected override async Task AdicionarToRepositoryAsync(Documento model, CancellationToken cancellationToken)
        {
            await _repository.AdicionarAsync(model, cancellationToken);
        }

        protected override async Task AtualizarToRepositoryAsync(Documento model, CancellationToken cancellationToken)
        {
            await _repository.AtualizarAsync(model, cancellationToken);
        }

        protected override async Task RemoverFromRepositoryAsync(int id, CancellationToken cancellationToken)
        {
            await _repository.RemoverAsync(id, cancellationToken);
        }

        #endregion

        #region Métodos específicos para Documentos

        /// <summary>
        /// Obtém documentos por veículo ID.
        /// </summary>
        public async Task<List<Documento>> ObterPorVeiculoIdAsync(int veiculoId, CancellationToken cancellationToken = default)
        {
            ValidateId(veiculoId, nameof(veiculoId));

            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                LogCrudOperation("ObterPorVeiculoId", _entityName, veiculoId);

                // Verificar cache
                var cached = GetFromCache<List<Documento>>($"{_entityName}_Veiculo_{veiculoId}");
                if (cached != null)
                {
                    return cached;
                }

                var result = await _repository.ObterPorVeiculoIdAsync(veiculoId, cancellationToken);
                
                // Cache por 5 minutos
                SetCache($"{_entityName}_Veiculo_{veiculoId}", result, TimeSpan.FromMinutes(5));
                
                LogCrudSuccess("ObterPorVeiculoId", _entityName, veiculoId, result.Count);
                return result;

            }, $"Obter documentos por veículo ID", cancellationToken);
        }

        /// <summary>
        /// Obtém documentos vencidos.
        /// </summary>
        public async Task<List<Documento>> ObterVencidosAsync(CancellationToken cancellationToken = default)
        {
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                LogCrudOperation("ObterVencidos", _entityName);

                var cached = GetFromCache<List<Documento>>($"{_entityName}_Vencidos");
                if (cached != null)
                {
                    return cached;
                }

                var result = await _repository.ObterVencidosAsync(cancellationToken);
                
                // Cache por 30 minutos (dados críticos, mas não mudam muito)
                SetCache($"{_entityName}_Vencidos", result, TimeSpan.FromMinutes(30));
                
                LogCrudSuccess("ObterVencidos", _entityName, count: result.Count);
                return result;

            }, "Obter documentos vencidos", cancellationToken);
        }

        /// <summary>
        /// Obtém documentos próximos do vencimento.
        /// </summary>
        public async Task<List<Documento>> ObterProximosDoVencimentoAsync(int dias = 30, CancellationToken cancellationToken = default)
        {
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                LogCrudOperation("ObterProximosDoVencimento", _entityName, dias);

                var cached = GetFromCache<List<Documento>>($"{_entityName}_ProximosVencimento_{dias}");
                if (cached != null)
                {
                    return cached;
                }

                var result = await _repository.ObterProximosDoVencimentoAsync(dias, cancellationToken);
                
                // Cache por 1 hora
                SetCache($"{_entityName}_ProximosVencimento_{dias}", result, TimeSpan.FromHours(1));
                
                LogCrudSuccess("ObterProximosDoVencimento", _entityName, dias, result.Count);
                return result;

            }, "Obter documentos próximos do vencimento", cancellationToken);
        }

        /// <summary>
        /// Marca um documento como notificado.
        /// </summary>
        public async Task MarcarComoNotificadoAsync(int documentoId, CancellationToken cancellationToken = default)
        {
            ValidateId(documentoId);

            await ExecuteWithErrorHandlingAsync(async () =>
            {
                LogCrudOperation("MarcarComoNotificado", _entityName, documentoId);

                await _repository.MarcarComoNotificadoAsync(documentoId, cancellationToken);
                
                // Invalidar cache do documento específico
                ClearCache($"{_entityName}_{documentoId}");
                
                LogCrudSuccess("MarcarComoNotificado", _entityName, documentoId);

            }, "Marcar documento como notificado", cancellationToken);
        }

        /// <summary>
        /// Conta documentos vencidos por veículo.
        /// </summary>
        public async Task<int> ContarDocumentosVencidosAsync(int veiculoId, CancellationToken cancellationToken = default)
        {
            ValidateId(veiculoId);

            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                LogCrudOperation("ContarDocumentosVencidos", _entityName, veiculoId);

                var cached = GetFromCache<int>($"{_entityName}_CountVencidos_{veiculoId}");
                if (cached > 0)
                {
                    return cached;
                }

                var count = await _repository.ContarDocumentosVencidosAsync(veiculoId, cancellationToken);
                
                // Cache por 1 hora
                SetCache($"{_entityName}_CountVencidos_{veiculoId}", count, TimeSpan.FromHours(1));
                
                LogCrudSuccess("ContarDocumentosVencidos", _entityName, veiculoId, count);
                return count;

            }, "Contar documentos vencidos", cancellationToken);
        }

        #endregion

        #region Validações específicas sobrescritas

        protected override async Task ValidateForAddAsync(Documento model, CancellationToken cancellationToken)
        {
            await base.ValidateForAddAsync(model, cancellationToken);

            // Validações específicas para documentos
            if (model.DataVencimento <= DateTime.Today)
            {
                throw new ArgumentException("Data de vencimento deve ser futura", nameof(model.DataVencimento));
            }

            ValidateNotNullOrEmpty(model.TipoDocumento, nameof(model.TipoDocumento));
            ValidateId(model.VeiculoId, nameof(model.VeiculoId));

            _logger.LogInformation("Validações específicas para adição de documento concluídas");
        }

        protected override async Task ValidateForUpdateAsync(Documento model, CancellationToken cancellationToken)
        {
            await base.ValidateForUpdateAsync(model, cancellationToken);

            // Verificar se o documento existe
            var exists = await ExisteAsync(model.Id, cancellationToken);
            if (!exists)
            {
                throw new ArgumentException($"Documento com ID {model.Id} não existe", nameof(model.Id));
            }

            // Validações específicas
            ValidateNotNullOrEmpty(model.TipoDocumento, nameof(model.TipoDocumento));
            ValidateId(model.VeiculoId, nameof(model.VeiculoId));

            _logger.LogInformation("Validações específicas para atualização de documento concluídas");
        }

        protected override async Task ValidateForDeleteAsync(int id, CancellationToken cancellationToken)
        {
            await base.ValidateForDeleteAsync(id, cancellationToken);

            // Verificar se o documento existe
            var exists = await ExisteAsync(id, cancellationToken);
            if (!exists)
            {
                throw new ArgumentException($"Documento com ID {id} não existe", nameof(id));
            }

            _logger.LogInformation("Validações específicas para exclusão de documento concluídas");
        }

        #endregion

        #region Invalidação de cache customizada

        protected override void InvalidateCache()
        {
            // Invalidar cache base
            base.InvalidateCache();

            // Invalidar caches específicos
            ClearCache("Vencidos");
            ClearCache("ProximosVencimento");
            ClearCache("CountVencidos");

            _logger.LogInformation("Cache de documentos completamente invalidado");
        }

        #endregion
    }
}
