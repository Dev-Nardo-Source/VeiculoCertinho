using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VeiculoCertinho.Models;
using VeiculoCertinho.Repositories;

namespace VeiculoCertinho.Services
{
    public class ServicoEsteticoService : BaseService
    {
        private readonly ServicoEsteticoRepositorio _repositorio;
        private readonly ILogger<ServicoEsteticoService> _logger;

        public ServicoEsteticoService(ServicoEsteticoRepositorio repositorio, ILogger<ServicoEsteticoService> logger) : base(logger)
        {
            _repositorio = repositorio;
            _logger = logger;
        }

        /// <summary>
        /// Obtém todos os serviços estéticos de forma assíncrona.
        /// </summary>
        /// <returns>Lista de objetos ServicoEstetico.</returns>
        public async Task<List<ServicoEstetico>> ObterTodosAsync()
        {
            try
            {
                return await _repositorio.ObterTodosAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter serviços estéticos.");
                throw;
            }
        }

        /// <summary>
        /// Adiciona um novo serviço estético de forma assíncrona.
        /// </summary>
        /// <param name="servico">Objeto ServicoEstetico a ser adicionado.</param>
        /// <returns>Task representando a operação assíncrona.</returns>
        public async Task AdicionarAsync(ServicoEstetico servico)
        {
            if (servico == null)
                throw new ArgumentNullException(nameof(servico));

            try
            {
                await _repositorio.AdicionarAsync(servico);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar serviço estético.");
                throw;
            }
        }

        /// <summary>
        /// Atualiza um serviço estético existente de forma assíncrona.
        /// </summary>
        /// <param name="servico">Objeto ServicoEstetico a ser atualizado.</param>
        /// <returns>Task representando a operação assíncrona.</returns>
        public async Task AtualizarAsync(ServicoEstetico servico)
        {
            if (servico == null)
                throw new ArgumentNullException(nameof(servico));

            try
            {
                await _repositorio.AtualizarAsync(servico);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar serviço estético.");
                throw;
            }
        }

        /// <summary>
        /// Remove um serviço estético pelo ID de forma assíncrona.
        /// </summary>
        /// <param name="id">ID do serviço estético a ser removido.</param>
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
                _logger.LogError(ex, "Erro ao remover serviço estético.");
                throw;
            }
        }
    }
}
