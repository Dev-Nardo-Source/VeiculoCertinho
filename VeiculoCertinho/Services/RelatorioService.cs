using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VeiculoCertinho.Models;
using VeiculoCertinho.Repositories;

namespace VeiculoCertinho.Services
{
    public class RelatorioService : BaseService
    {
        private readonly RelatorioRepositorio _repositorio;
        private readonly ILogger<RelatorioService> _logger;

        public RelatorioService(RelatorioRepositorio repositorio, ILogger<RelatorioService> logger) : base(logger)
        {
            _repositorio = repositorio;
            _logger = logger;
        }

        /// <summary>
        /// Obtém todos os relatórios de forma assíncrona.
        /// </summary>
        /// <returns>Lista de objetos Relatorio.</returns>
        public async Task<List<Relatorio>> ObterTodosAsync()
        {
            try
            {
                return await _repositorio.ObterTodosAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter relatórios.");
                throw;
            }
        }

        /// <summary>
        /// Adiciona um novo relatório de forma assíncrona.
        /// </summary>
        /// <param name="relatorio">Objeto Relatorio a ser adicionado.</param>
        /// <returns>Task representando a operação assíncrona.</returns>
        public async Task AdicionarAsync(Relatorio relatorio)
        {
            if (relatorio == null)
                throw new ArgumentNullException(nameof(relatorio));

            try
            {
                await _repositorio.AdicionarAsync(relatorio);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar relatório.");
                throw;
            }
        }

        /// <summary>
        /// Atualiza um relatório existente de forma assíncrona.
        /// </summary>
        /// <param name="relatorio">Objeto Relatorio a ser atualizado.</param>
        /// <returns>Task representando a operação assíncrona.</returns>
        public async Task AtualizarAsync(Relatorio relatorio)
        {
            if (relatorio == null)
                throw new ArgumentNullException(nameof(relatorio));

            try
            {
                await _repositorio.AtualizarAsync(relatorio);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar relatório.");
                throw;
            }
        }

        /// <summary>
        /// Remove um relatório pelo ID de forma assíncrona.
        /// </summary>
        /// <param name="id">ID do relatório a ser removido.</param>
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
                _logger.LogError(ex, "Erro ao remover relatório.");
                throw;
            }
        }
    }
}
