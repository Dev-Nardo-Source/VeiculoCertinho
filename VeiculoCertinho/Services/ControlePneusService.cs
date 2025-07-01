using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VeiculoCertinho.Models;
using VeiculoCertinho.Repositories;

namespace VeiculoCertinho.Services
{
    public class ControlePneusService : BaseService
    {
        private readonly ControlePneusRepositorio _repositorio;
        private readonly ILogger<ControlePneusService> _logger;

        public ControlePneusService(ControlePneusRepositorio repositorio, ILogger<ControlePneusService> logger) : base(logger)
        {
            _repositorio = repositorio;
            _logger = logger;
        }

        /// <summary>
        /// Obtém todos os controles de pneus de forma assíncrona.
        /// </summary>
        /// <returns>Lista de objetos ControlePneus.</returns>
        public async Task<List<ControlePneus>> ObterTodosAsync()
        {
            try
            {
                return await _repositorio.ObterTodosAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter controles de pneus.");
                throw;
            }
        }

        /// <summary>
        /// Adiciona um novo controle de pneu de forma assíncrona.
        /// </summary>
        /// <param name="pneu">Objeto ControlePneus a ser adicionado.</param>
        /// <returns>Task representando a operação assíncrona.</returns>
        public async Task AdicionarAsync(ControlePneus pneu)
        {
            if (pneu == null)
                throw new ArgumentNullException(nameof(pneu));

            try
            {
                await _repositorio.AdicionarAsync(pneu);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar controle de pneus.");
                throw;
            }
        }

        /// <summary>
        /// Atualiza um controle de pneu existente de forma assíncrona.
        /// </summary>
        /// <param name="pneu">Objeto ControlePneus a ser atualizado.</param>
        /// <returns>Task representando a operação assíncrona.</returns>
        public async Task AtualizarAsync(ControlePneus pneu)
        {
            if (pneu == null)
                throw new ArgumentNullException(nameof(pneu));

            try
            {
                await _repositorio.AtualizarAsync(pneu);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar controle de pneus.");
                throw;
            }
        }

        /// <summary>
        /// Remove um controle de pneu pelo ID de forma assíncrona.
        /// </summary>
        /// <param name="id">ID do controle de pneu a ser removido.</param>
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
                _logger.LogError(ex, "Erro ao remover controle de pneus.");
                throw;
            }
        }
    }
}
