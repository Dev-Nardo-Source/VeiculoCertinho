using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VeiculoCertinho.Models;
using VeiculoCertinho.Repositories;

namespace VeiculoCertinho.Services
{
    public class ManutencaoPreventivaService : BaseService
    {
        private readonly ManutencaoPreventivaRepositorio _repositorio;
        private readonly ILogger<ManutencaoPreventivaService> _logger;

        public ManutencaoPreventivaService(ManutencaoPreventivaRepositorio repositorio, ILogger<ManutencaoPreventivaService> logger) : base(logger)
        {
            _repositorio = repositorio;
            _logger = logger;
        }

        /// <summary>
        /// Obtém todas as manutenções preventivas de forma assíncrona.
        /// </summary>
        /// <returns>Lista de objetos ManutencaoPreventiva.</returns>
        public async Task<List<ManutencaoPreventiva>> ObterTodosAsync()
        {
            try
            {
                return await _repositorio.ObterTodosAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter manutenções preventivas.");
                throw;
            }
        }

        /// <summary>
        /// Adiciona uma nova manutenção preventiva de forma assíncrona.
        /// </summary>
        /// <param name="manutencao">Objeto ManutencaoPreventiva a ser adicionado.</param>
        /// <returns>Task representando a operação assíncrona.</returns>
        public async Task AdicionarAsync(ManutencaoPreventiva manutencao)
        {
            if (manutencao == null)
                throw new ArgumentNullException(nameof(manutencao));

            try
            {
                await _repositorio.AdicionarAsync(manutencao);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar manutenção preventiva.");
                throw;
            }
        }

        /// <summary>
        /// Atualiza uma manutenção preventiva existente de forma assíncrona.
        /// </summary>
        /// <param name="manutencao">Objeto ManutencaoPreventiva a ser atualizado.</param>
        /// <returns>Task representando a operação assíncrona.</returns>
        public async Task AtualizarAsync(ManutencaoPreventiva manutencao)
        {
            if (manutencao == null)
                throw new ArgumentNullException(nameof(manutencao));

            try
            {
                await _repositorio.AtualizarAsync(manutencao);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar manutenção preventiva.");
                throw;
            }
        }

        /// <summary>
        /// Remove uma manutenção preventiva pelo ID de forma assíncrona.
        /// </summary>
        /// <param name="id">ID da manutenção preventiva a ser removida.</param>
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
                _logger.LogError(ex, "Erro ao remover manutenção preventiva.");
                throw;
            }
        }
    }
}
