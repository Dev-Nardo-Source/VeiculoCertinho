using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VeiculoCertinho.Models;
using VeiculoCertinho.Repositories;

namespace VeiculoCertinho.Services
{
    public class DashboardService : BaseService
    {
        private readonly DashboardRepositorio _repositorio;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(DashboardRepositorio repositorio, ILogger<DashboardService> logger) : base(logger)
        {
            _repositorio = repositorio;
            _logger = logger;
        }

        /// <summary>
        /// Obtém todos os indicadores do dashboard de forma assíncrona.
        /// </summary>
        /// <returns>Lista de objetos DashboardIndicador.</returns>
        public async Task<List<DashboardIndicador>> ObterTodosAsync()
        {
            try
            {
                return await _repositorio.ObterTodosAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter indicadores do dashboard.");
                throw;
            }
        }

        /// <summary>
        /// Adiciona um novo indicador ao dashboard de forma assíncrona.
        /// </summary>
        /// <param name="indicador">Objeto DashboardIndicador a ser adicionado.</param>
        /// <returns>Task representando a operação assíncrona.</returns>
        public async Task AdicionarAsync(DashboardIndicador indicador)
        {
            if (indicador == null)
                throw new ArgumentNullException(nameof(indicador));

            try
            {
                await _repositorio.AdicionarAsync(indicador);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar indicador ao dashboard.");
                throw;
            }
        }

        /// <summary>
        /// Atualiza um indicador existente do dashboard de forma assíncrona.
        /// </summary>
        /// <param name="indicador">Objeto DashboardIndicador a ser atualizado.</param>
        /// <returns>Task representando a operação assíncrona.</returns>
        public async Task AtualizarAsync(DashboardIndicador indicador)
        {
            if (indicador == null)
                throw new ArgumentNullException(nameof(indicador));

            try
            {
                await _repositorio.AtualizarAsync(indicador);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar indicador do dashboard.");
                throw;
            }
        }

        /// <summary>
        /// Remove um indicador do dashboard pelo ID de forma assíncrona.
        /// </summary>
        /// <param name="id">ID do indicador a ser removido.</param>
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
                _logger.LogError(ex, "Erro ao remover indicador do dashboard.");
                throw;
            }
        }
    }
}
