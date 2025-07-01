using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace VeiculoCertinho.Services
{
    /// <summary>
    /// Serviço genérico que implementa operações CRUD padrão para qualquer repositório.
    /// Elimina código boilerplate repetitivo em Services específicos.
    /// </summary>
    public abstract class GenericService<TModel, TRepository> : BaseService
        where TModel : class
        where TRepository : class
    {
        protected readonly TRepository _repository;
        protected readonly string _entityName;

        protected GenericService(TRepository repository, ILogger logger) : base(logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _entityName = typeof(TModel).Name;
        }

        /// <summary>
        /// Obtém todos os registros de forma assíncrona com cache automático.
        /// </summary>
        public virtual async Task<List<TModel>> ObterTodosAsync(bool useCache = true, CancellationToken cancellationToken = default)
        {
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                LogCrudOperation("ObterTodos", _entityName);

                // Verificar cache primeiro
                if (useCache)
                {
                    var cached = GetFromCache<List<TModel>>($"{_entityName}_All");
                    if (cached != null)
                    {
                        _logger.LogInformation("Dados obtidos do cache para {EntityName}", _entityName);
                        return cached;
                    }
                }

                // Obter do repositório
                var result = await ObterTodosFromRepositoryAsync(cancellationToken);
                
                // Armazenar no cache
                if (useCache && result != null)
                {
                    SetCache($"{_entityName}_All", result, TimeSpan.FromMinutes(10));
                }

                LogCrudSuccess("ObterTodos", _entityName, count: result?.Count);
                return result ?? new List<TModel>();

            }, $"Obter todos {_entityName}", cancellationToken);
        }

        /// <summary>
        /// Obtém um registro por ID de forma assíncrona com cache automático.
        /// </summary>
        public virtual async Task<TModel?> ObterPorIdAsync(int id, bool useCache = true, CancellationToken cancellationToken = default)
        {
            ValidateId(id);

            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                LogCrudOperation("ObterPorId", _entityName, id);

                // Verificar cache primeiro
                if (useCache)
                {
                    var cached = GetFromCache<TModel>($"{_entityName}_{id}");
                    if (cached != null)
                    {
                        _logger.LogInformation("Dados obtidos do cache para {EntityName} ID: {Id}", _entityName, id);
                        return cached;
                    }
                }

                // Obter do repositório
                var result = await ObterPorIdFromRepositoryAsync(id, cancellationToken);

                // Armazenar no cache
                if (useCache && result != null)
                {
                    SetCache($"{_entityName}_{id}", result, TimeSpan.FromMinutes(15));
                }

                LogCrudSuccess("ObterPorId", _entityName, id);
                return result;

            }, $"Obter {_entityName} por ID", cancellationToken);
        }

        /// <summary>
        /// Adiciona um novo registro de forma assíncrona com invalidação automática de cache.
        /// </summary>
        public virtual async Task AdicionarAsync(TModel model, CancellationToken cancellationToken = default)
        {
            ValidateNotNull(model, nameof(model));

            await ExecuteWithErrorHandlingAsync(async () =>
            {
                LogCrudOperation("Adicionar", _entityName);

                // Validações adicionais
                await ValidateForAddAsync(model, cancellationToken);

                // Adicionar no repositório
                await AdicionarToRepositoryAsync(model, cancellationToken);

                // Invalidar cache
                InvalidateCache();

                LogCrudSuccess("Adicionar", _entityName);

            }, $"Adicionar {_entityName}", cancellationToken);
        }

        /// <summary>
        /// Atualiza um registro existente de forma assíncrona com invalidação automática de cache.
        /// </summary>
        public virtual async Task AtualizarAsync(TModel model, CancellationToken cancellationToken = default)
        {
            ValidateNotNull(model, nameof(model));

            await ExecuteWithErrorHandlingAsync(async () =>
            {
                LogCrudOperation("Atualizar", _entityName);

                // Validações adicionais
                await ValidateForUpdateAsync(model, cancellationToken);

                // Atualizar no repositório
                await AtualizarToRepositoryAsync(model, cancellationToken);

                // Invalidar cache
                InvalidateCache();

                LogCrudSuccess("Atualizar", _entityName);

            }, $"Atualizar {_entityName}", cancellationToken);
        }

        /// <summary>
        /// Remove um registro por ID de forma assíncrona com invalidação automática de cache.
        /// </summary>
        public virtual async Task RemoverAsync(int id, CancellationToken cancellationToken = default)
        {
            ValidateId(id);

            await ExecuteWithErrorHandlingAsync(async () =>
            {
                LogCrudOperation("Remover", _entityName, id);

                // Validações adicionais
                await ValidateForDeleteAsync(id, cancellationToken);

                // Remover do repositório
                await RemoverFromRepositoryAsync(id, cancellationToken);

                // Invalidar cache
                InvalidateCache();

                LogCrudSuccess("Remover", _entityName, id);

            }, $"Remover {_entityName}", cancellationToken);
        }

        /// <summary>
        /// Obtém contagem total de registros.
        /// </summary>
        public virtual async Task<int> ContarAsync(CancellationToken cancellationToken = default)
        {
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                LogCrudOperation("Contar", _entityName);

                var cached = GetFromCache<int>($"{_entityName}_Count");
                if (cached > 0)
                {
                    return cached;
                }

                var count = await ContarFromRepositoryAsync(cancellationToken);
                SetCache($"{_entityName}_Count", count, TimeSpan.FromMinutes(5));

                LogCrudSuccess("Contar", _entityName, count: count);
                return count;

            }, $"Contar {_entityName}", cancellationToken);
        }

        /// <summary>
        /// Verifica se um registro existe por ID.
        /// </summary>
        public virtual async Task<bool> ExisteAsync(int id, CancellationToken cancellationToken = default)
        {
            ValidateId(id);

            var cached = GetFromCache<bool>($"{_entityName}_Exists_{id}");
            if (cached)
            {
                return true;
            }

            var exists = await ExisteFromRepositoryAsync(id, cancellationToken);
            if (exists)
            {
                SetCache($"{_entityName}_Exists_{id}", true, TimeSpan.FromMinutes(5));
            }

            return exists;
        }

        /// <summary>
        /// Invalida todo o cache relacionado à entidade.
        /// </summary>
        protected virtual void InvalidateCache()
        {
            ClearCache(_entityName);
        }

        #region Métodos abstratos - devem ser implementados pelas classes filhas

        /// <summary>
        /// Obtém todos os registros do repositório. Deve ser implementado pela classe filha.
        /// </summary>
        protected abstract Task<List<TModel>> ObterTodosFromRepositoryAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Obtém um registro por ID do repositório. Deve ser implementado pela classe filha.
        /// </summary>
        protected abstract Task<TModel?> ObterPorIdFromRepositoryAsync(int id, CancellationToken cancellationToken);

        /// <summary>
        /// Adiciona um registro no repositório. Deve ser implementado pela classe filha.
        /// </summary>
        protected abstract Task AdicionarToRepositoryAsync(TModel model, CancellationToken cancellationToken);

        /// <summary>
        /// Atualiza um registro no repositório. Deve ser implementado pela classe filha.
        /// </summary>
        protected abstract Task AtualizarToRepositoryAsync(TModel model, CancellationToken cancellationToken);

        /// <summary>
        /// Remove um registro do repositório. Deve ser implementado pela classe filha.
        /// </summary>
        protected abstract Task RemoverFromRepositoryAsync(int id, CancellationToken cancellationToken);

        #endregion

        #region Métodos virtuais - podem ser sobrescritos pelas classes filhas

        /// <summary>
        /// Validações específicas antes de adicionar. Pode ser sobrescrito pela classe filha.
        /// </summary>
        protected virtual Task ValidateForAddAsync(TModel model, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Validações específicas antes de atualizar. Pode ser sobrescrito pela classe filha.
        /// </summary>
        protected virtual Task ValidateForUpdateAsync(TModel model, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Validações específicas antes de deletar. Pode ser sobrescrito pela classe filha.
        /// </summary>
        protected virtual Task ValidateForDeleteAsync(int id, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Conta registros no repositório. Implementação padrão usando ObterTodos.
        /// </summary>
        protected virtual async Task<int> ContarFromRepositoryAsync(CancellationToken cancellationToken)
        {
            var all = await ObterTodosFromRepositoryAsync(cancellationToken);
            return all?.Count ?? 0;
        }

        /// <summary>
        /// Verifica se existe no repositório. Implementação padrão usando ObterPorId.
        /// </summary>
        protected virtual async Task<bool> ExisteFromRepositoryAsync(int id, CancellationToken cancellationToken)
        {
            var item = await ObterPorIdFromRepositoryAsync(id, cancellationToken);
            return item != null;
        }

        #endregion
    }
} 