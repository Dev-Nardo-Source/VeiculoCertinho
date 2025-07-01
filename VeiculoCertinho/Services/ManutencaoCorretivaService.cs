using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VeiculoCertinho.Models;
using VeiculoCertinho.Repositories;

namespace VeiculoCertinho.Services
{
    public class ManutencaoCorretivaService : BaseService
    {
        private readonly ManutencaoCorretivaRepositorio _repositorio;
        private readonly ILogger<ManutencaoCorretivaService> _logger;

        public ManutencaoCorretivaService(ManutencaoCorretivaRepositorio repositorio, ILogger<ManutencaoCorretivaService> logger) : base(logger)
        {
            _repositorio = repositorio;
            _logger = logger;
        }

        /// <summary>
        /// Obtém todas as manutenções corretivas de forma assíncrona.
        /// </summary>
        /// <returns>Lista de objetos ManutencaoCorretiva.</returns>
        public async Task<List<ManutencaoCorretiva>> ObterTodosAsync()
        {
            try
            {
                return await _repositorio.ObterTodosAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter manutenções corretivas.");
                throw;
            }
        }

        /// <summary>
        /// Adiciona uma nova manutenção corretiva de forma assíncrona.
        /// </summary>
        /// <param name="manutencao">Objeto ManutencaoCorretiva a ser adicionado.</param>
        /// <returns>Task representando a operação assíncrona.</returns>
        public async Task AdicionarAsync(ManutencaoCorretiva manutencao)
        {
            if (manutencao == null)
                throw new ArgumentNullException(nameof(manutencao));

            try
            {
                await _repositorio.AdicionarAsync(manutencao);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar manutenção corretiva.");
                throw;
            }
        }

        /// <summary>
        /// Atualiza uma manutenção corretiva existente de forma assíncrona.
        /// </summary>
        /// <param name="manutencao">Objeto ManutencaoCorretiva a ser atualizado.</param>
        /// <returns>Task representando a operação assíncrona.</returns>
        public async Task AtualizarAsync(ManutencaoCorretiva manutencao)
        {
            if (manutencao == null)
                throw new ArgumentNullException(nameof(manutencao));

            try
            {
                await _repositorio.AtualizarAsync(manutencao);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar manutenção corretiva.");
                throw;
            }
        }

        /// <summary>
        /// Remove uma manutenção corretiva pelo ID de forma assíncrona.
        /// </summary>
        /// <param name="id">ID da manutenção corretiva a ser removida.</param>
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
                _logger.LogError(ex, "Erro ao remover manutenção corretiva.");
                throw;
            }
        }

        /// <summary>
        /// Conta a frequência de problemas descritos para identificar falhas recorrentes.
        /// </summary>
        /// <param name="manutencoes">Lista de manutenções corretivas para análise.</param>
        /// <returns>Dicionário com a descrição da falha e sua frequência.</returns>
        public Dictionary<string, int> AnalisarFalhasRecorrentes(List<ManutencaoCorretiva> manutencoes)
        {
            if (manutencoes == null)
                return new Dictionary<string, int>();

            return manutencoes
                .GroupBy(m => m.DescricaoFalha)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        /// <summary>
        /// Sugere prevenções baseadas em falhas recorrentes.
        /// </summary>
        /// <param name="manutencoes">Lista de manutenções corretivas para análise.</param>
        /// <returns>Lista de sugestões de prevenção baseadas em falhas recorrentes.</returns>
        public List<string> SugerirPrevencoes(List<ManutencaoCorretiva> manutencoes)
        {
            var falhas = AnalisarFalhasRecorrentes(manutencoes);
            var sugestoes = new List<string>();

            foreach (var falha in falhas)
            {
                if (falha.Value > 1)
                {
                    sugestoes.Add($"Atenção: o problema '{falha.Key}' ocorreu {falha.Value} vezes. Considere verificar componentes relacionados para evitar recorrência.");
                }
            }

            return sugestoes;
        }
    }
}
