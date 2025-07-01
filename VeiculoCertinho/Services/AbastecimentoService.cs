using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VeiculoCertinho.Models;
using VeiculoCertinho.Repositories;
using Microsoft.Extensions.Configuration;

namespace VeiculoCertinho.Services
{
    public class AbastecimentoService : BaseService
    {
        private readonly AbastecimentoRepositorio _repositorio;
        private readonly ILogger<AbastecimentoService> _logger;

        public AbastecimentoService(IConfiguration configuration, ILogger<AbastecimentoService> logger) : base(logger)
        {
            _logger = logger;
            var repositoryLogger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<AbastecimentoRepositorio>();
            _repositorio = new AbastecimentoRepositorio(configuration, repositoryLogger);
        }

        /// <summary>
        /// Obtém todos os abastecimentos de forma assíncrona.
        /// </summary>
        /// <returns>Lista de objetos Abastecimento.</returns>
        public async Task<List<Abastecimento>> ObterTodosAsync()
        {
            try
            {
                return await _repositorio.ObterTodosAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter abastecimentos.");
                throw;
            }
        }

        /// <summary>
        /// Adiciona um novo abastecimento de forma assíncrona.
        /// </summary>
        /// <param name="abastecimento">Objeto Abastecimento a ser adicionado.</param>
        /// <returns>Task representando a operação assíncrona.</returns>
        public async Task AdicionarAsync(Abastecimento? abastecimento)
        {
            if (abastecimento == null)
                throw new ArgumentNullException(nameof(abastecimento));

            try
            {
                await _repositorio.AdicionarAsync(abastecimento);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar abastecimento.");
                throw;
            }
        }

        /// <summary>
        /// Atualiza um abastecimento existente de forma assíncrona.
        /// </summary>
        /// <param name="abastecimento">Objeto Abastecimento a ser atualizado.</param>
        /// <returns>Task representando a operação assíncrona.</returns>
        public async Task AtualizarAsync(Abastecimento? abastecimento)
        {
            if (abastecimento == null)
                throw new ArgumentNullException(nameof(abastecimento));

            try
            {
                await _repositorio.AtualizarAsync(abastecimento);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar abastecimento.");
                throw;
            }
        }

        /// <summary>
        /// Remove um abastecimento pelo ID de forma assíncrona.
        /// </summary>
        /// <param name="id">ID do abastecimento a ser removido.</param>
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
                _logger.LogError(ex, "Erro ao remover abastecimento.");
                throw;
            }
        }

        /// <summary>
        /// Calcula o consumo de km por unidade de combustível.
        /// </summary>
        /// <param name="abastecimentos">Lista de abastecimentos para cálculo.</param>
        /// <param name="tipoCombustivel">Tipo de combustível a ser considerado.</param>
        /// <returns>Consumo em km por unidade de combustível.</returns>
        public decimal CalcularConsumoKmPorUnidade(List<Abastecimento> abastecimentos, string tipoCombustivel)
        {
            if (abastecimentos == null || abastecimentos.Count < 2 || string.IsNullOrEmpty(tipoCombustivel))
                return 0;

            decimal totalQuantidade = 0;
            int quilometragemInicial = abastecimentos[0].QuilometragemAtual;
            int quilometragemFinal = abastecimentos[abastecimentos.Count - 1].QuilometragemAtual;

            foreach (var abastecimento in abastecimentos)
            {
                if (abastecimento.TipoCombustivel == tipoCombustivel)
                {
                    totalQuantidade += abastecimento.Quantidade;
                }
            }

            int distancia = quilometragemFinal - quilometragemInicial;
            if (totalQuantidade == 0 || distancia <= 0)
                return 0;

            return (decimal)distancia / totalQuantidade;
        }

        /// <summary>
        /// Compara combustíveis aceitos e retorna o mais vantajoso baseado no consumo e preço.
        /// </summary>
        /// <param name="abastecimentos">Lista de abastecimentos para análise.</param>
        /// <param name="combustiveisAceitos">Lista de combustíveis aceitos.</param>
        /// <param name="precos">Dicionário com preços por tipo de combustível.</param>
        /// <returns>Combustível mais vantajoso.</returns>
        public string? CompararCombustiveis(List<Abastecimento>? abastecimentos, List<string>? combustiveisAceitos, Dictionary<string, decimal>? precos)
        {
            if (abastecimentos == null || combustiveisAceitos == null || precos == null)
                return null;

            decimal melhorCustoPorKm = decimal.MaxValue;
            string? melhorCombustivel = null;

            foreach (var combustivel in combustiveisAceitos)
            {
                decimal consumo = CalcularConsumoKmPorUnidade(abastecimentos, combustivel);
                if (consumo == 0 || !precos.ContainsKey(combustivel))
                    continue;

                decimal custoPorKm = precos[combustivel] / consumo;
                if (custoPorKm < melhorCustoPorKm)
                {
                    melhorCustoPorKm = custoPorKm;
                    melhorCombustivel = combustivel;
                }
            }

            return melhorCombustivel;
        }
    }
}
