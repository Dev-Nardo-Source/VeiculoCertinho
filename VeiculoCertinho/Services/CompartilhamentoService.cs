using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VeiculoCertinho.Models;
using VeiculoCertinho.Repositories;

namespace VeiculoCertinho.Services
{
    public class CompartilhamentoService : BaseService
    {
        private readonly CompartilhamentoRepositorio _repositorio;
        private readonly ILogger<CompartilhamentoService> _logger;

        public CompartilhamentoService(CompartilhamentoRepositorio repositorio, ILogger<CompartilhamentoService> logger) : base(logger)
        {
            _repositorio = repositorio;
            _logger = logger;
        }

        /// <summary>
        /// Obtém todos os compartilhamentos de forma assíncrona.
        /// </summary>
        /// <returns>Lista de objetos Compartilhamento.</returns>
        public async Task<List<Compartilhamento>> ObterTodosAsync()
        {
            try
            {
                return await _repositorio.ObterTodosAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter compartilhamentos.");
                throw;
            }
        }

        /// <summary>
        /// Adiciona um novo compartilhamento de forma assíncrona.
        /// </summary>
        /// <param name="compartilhamento">Objeto Compartilhamento a ser adicionado.</param>
        /// <returns>Task representando a operação assíncrona.</returns>
        public async Task AdicionarAsync(Compartilhamento compartilhamento)
        {
            if (compartilhamento == null)
                throw new ArgumentNullException(nameof(compartilhamento));

            try
            {
                await _repositorio.AdicionarAsync(compartilhamento);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar compartilhamento.");
                throw;
            }
        }

        /// <summary>
        /// Atualiza um compartilhamento existente de forma assíncrona.
        /// </summary>
        /// <param name="compartilhamento">Objeto Compartilhamento a ser atualizado.</param>
        /// <returns>Task representando a operação assíncrona.</returns>
        public async Task AtualizarAsync(Compartilhamento compartilhamento)
        {
            if (compartilhamento == null)
                throw new ArgumentNullException(nameof(compartilhamento));

            try
            {
                await _repositorio.AtualizarAsync(compartilhamento);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar compartilhamento.");
                throw;
            }
        }

        /// <summary>
        /// Remove um compartilhamento pelo ID de forma assíncrona.
        /// </summary>
        /// <param name="id">ID do compartilhamento a ser removido.</param>
        /// <returns>Task representando a operação assíncrona.</returns>
        public async Task RemoverAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "ID deve ser maior que zero.");

            try
            {
                await _repositorio.RemoverAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover compartilhamento.");
                throw;
            }
        }
    }
}
