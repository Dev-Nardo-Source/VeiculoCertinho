using System.Collections.Generic;
using System.Threading.Tasks;
using VeiculoCertinho.Models;
using VeiculoCertinho.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace VeiculoCertinho.Services
{
    public class VeiculoDetalhesService : BaseService
    {
        private readonly VeiculoDetalhesRepositorio _repositorio;

        public VeiculoDetalhesService(VeiculoDetalhesRepositorio repositorio, IConfiguration configuration, ILogger<VeiculoDetalhesService> logger) : base(logger)
        {
            _repositorio = repositorio;
        }

        /// <summary>
        /// Obtém todos os detalhes de veículos de forma assíncrona.
        /// </summary>
        public async Task<List<VeiculoDetalhes>> ObterTodosAsync()
        {
            return await _repositorio.ObterTodosAsync();
        }

        /// <summary>
        /// Adiciona novos detalhes de veículo de forma assíncrona.
        /// </summary>
        public async Task AdicionarAsync(VeiculoDetalhes detalhes)
        {
            if (detalhes == null)
                throw new System.ArgumentNullException(nameof(detalhes));

            await _repositorio.AdicionarAsync(detalhes);
        }

        /// <summary>
        /// Atualiza detalhes de veículo existentes de forma assíncrona.
        /// </summary>
        public async Task AtualizarAsync(VeiculoDetalhes detalhes)
        {
            if (detalhes == null)
                throw new System.ArgumentNullException(nameof(detalhes));

            await _repositorio.AtualizarAsync(detalhes);
        }

        /// <summary>
        /// Remove detalhes de veículo pelo ID de forma assíncrona.
        /// </summary>
        public async Task RemoverAsync(int id)
        {
            if (id <= 0)
                throw new System.ArgumentOutOfRangeException(nameof(id), "ID deve ser maior que zero.");

            await _repositorio.RemoverAsync(id);
        }
    }
}
