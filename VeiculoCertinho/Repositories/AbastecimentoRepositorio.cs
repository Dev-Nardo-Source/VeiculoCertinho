using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VeiculoCertinho.Models;
using Microsoft.Data.Sqlite;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Linq;

namespace VeiculoCertinho.Repositories
{
    public class AbastecimentoRepositorio : BaseRepositorio
    {
        private readonly IConfiguration _configuration;

        public AbastecimentoRepositorio(IConfiguration configuration, ILogger<AbastecimentoRepositorio> logger)
            : base(logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<List<Abastecimento>> ObterTodosAsync(CancellationToken cancellationToken = default)
        {
            const string query = @"
                SELECT Id, VeiculoId, Data, TipoCombustivel, PrecoPorUnidade, Quantidade, QuilometragemAtual, Posto, ValorTotal
                FROM Abastecimentos
                ORDER BY Data DESC";

            return await ExecuteQueryAsync(query, MapAbastecimento, cancellationToken: cancellationToken);
        }

        public async Task<Abastecimento?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
        {
            const string query = @"
                SELECT Id, VeiculoId, Data, TipoCombustivel, PrecoPorUnidade, Quantidade, QuilometragemAtual, Posto, ValorTotal
                FROM Abastecimentos
                WHERE Id = $id";

            var parameters = new Dictionary<string, object> { { "id", id } };
            return await ExecuteQuerySingleAsync(query, MapAbastecimento, parameters, cancellationToken);
        }

        public async Task<List<Abastecimento>> ObterPorVeiculoIdAsync(int veiculoId, CancellationToken cancellationToken = default)
        {
            const string query = @"
                SELECT Id, VeiculoId, Data, TipoCombustivel, PrecoPorUnidade, Quantidade, QuilometragemAtual, Posto, ValorTotal
                FROM Abastecimentos
                WHERE VeiculoId = $veiculoId
                ORDER BY Data DESC";

            var parameters = new Dictionary<string, object> { { "veiculoId", veiculoId } };
            return await ExecuteQueryAsync(query, MapAbastecimento, parameters, cancellationToken);
        }

        public async Task<List<Abastecimento>> ObterPorTipoCombustivelAsync(string tipoCombustivel, CancellationToken cancellationToken = default)
        {
            const string query = @"
                SELECT Id, VeiculoId, Data, TipoCombustivel, PrecoPorUnidade, Quantidade, QuilometragemAtual, Posto, ValorTotal
                FROM Abastecimentos
                WHERE TipoCombustivel = $tipoCombustivel
                ORDER BY Data DESC";

            var parameters = new Dictionary<string, object> { { "tipoCombustivel", tipoCombustivel } };
            return await ExecuteQueryAsync(query, MapAbastecimento, parameters, cancellationToken);
        }

        public async Task<List<Abastecimento>> ObterPorPeriodoAsync(DateTime dataInicio, DateTime dataFim, CancellationToken cancellationToken = default)
        {
            const string query = @"
                SELECT Id, VeiculoId, Data, TipoCombustivel, PrecoPorUnidade, Quantidade, QuilometragemAtual, Posto, ValorTotal
                FROM Abastecimentos
                WHERE Data BETWEEN $dataInicio AND $dataFim
                ORDER BY Data DESC";

            var parameters = new Dictionary<string, object> 
            { 
                { "dataInicio", dataInicio },
                { "dataFim", dataFim }
            };
            return await ExecuteQueryAsync(query, MapAbastecimento, parameters, cancellationToken);
        }

        public async Task<Abastecimento?> ObterUltimoAbastecimentoAsync(int veiculoId, CancellationToken cancellationToken = default)
        {
            const string query = @"
                SELECT Id, VeiculoId, Data, TipoCombustivel, PrecoPorUnidade, Quantidade, QuilometragemAtual, Posto, ValorTotal
                FROM Abastecimentos
                WHERE VeiculoId = $veiculoId
                ORDER BY Data DESC, Id DESC
                LIMIT 1";

            var parameters = new Dictionary<string, object> { { "veiculoId", veiculoId } };
            return await ExecuteQuerySingleAsync(query, MapAbastecimento, parameters, cancellationToken);
        }

        public async Task AdicionarAsync(Abastecimento abastecimento, CancellationToken cancellationToken = default)
        {
            const string command = @"
                INSERT INTO Abastecimentos (VeiculoId, Data, TipoCombustivel, PrecoPorUnidade, Quantidade, QuilometragemAtual, Posto, ValorTotal)
                VALUES ($veiculoId, $data, $tipoCombustivel, $precoPorUnidade, $quantidade, $quilometragemAtual, $posto, $valorTotal)
                RETURNING Id";

            var parameters = new Dictionary<string, object>
            {
                { "veiculoId", abastecimento.VeiculoId },
                { "data", abastecimento.Data },
                { "tipoCombustivel", abastecimento.TipoCombustivel },
                { "precoPorUnidade", abastecimento.PrecoPorUnidade },
                { "quantidade", abastecimento.Quantidade },
                { "quilometragemAtual", abastecimento.QuilometragemAtual },
                { "posto", abastecimento.Posto },
                { "valorTotal", abastecimento.ValorTotal }
            };

            var novoId = await ExecuteQuerySingleStructAsync(command, reader => GetSafeValue<int>(reader, 0), parameters, cancellationToken);
            if (novoId.HasValue)
            {
                abastecimento.Id = novoId.Value;
            }
        }

        public async Task AtualizarAsync(Abastecimento abastecimento, CancellationToken cancellationToken = default)
        {
            const string command = @"
                UPDATE Abastecimentos
                SET VeiculoId = $veiculoId,
                    Data = $data,
                    TipoCombustivel = $tipoCombustivel,
                    PrecoPorUnidade = $precoPorUnidade,
                    Quantidade = $quantidade,
                    QuilometragemAtual = $quilometragemAtual,
                    Posto = $posto,
                    ValorTotal = $valorTotal
                WHERE Id = $id";

            var parameters = new Dictionary<string, object>
            {
                { "id", abastecimento.Id },
                { "veiculoId", abastecimento.VeiculoId },
                { "data", abastecimento.Data },
                { "tipoCombustivel", abastecimento.TipoCombustivel },
                { "precoPorUnidade", abastecimento.PrecoPorUnidade },
                { "quantidade", abastecimento.Quantidade },
                { "quilometragemAtual", abastecimento.QuilometragemAtual },
                { "posto", abastecimento.Posto },
                { "valorTotal", abastecimento.ValorTotal }
            };

            await ExecuteCommandAsync(command, parameters, cancellationToken);
        }

        public async Task RemoverAsync(int id, CancellationToken cancellationToken = default)
        {
            const string command = "DELETE FROM Abastecimentos WHERE Id = $id";
            var parameters = new Dictionary<string, object> { { "id", id } };
            await ExecuteCommandAsync(command, parameters, cancellationToken);
        }

        // Métodos para estatísticas e relatórios
        public async Task<decimal> ObterGastoTotalAsync(int veiculoId, CancellationToken cancellationToken = default)
        {
            const string query = @"
                SELECT COALESCE(SUM(ValorTotal), 0)
                FROM Abastecimentos
                WHERE VeiculoId = $veiculoId";

            var parameters = new Dictionary<string, object> { { "veiculoId", veiculoId } };
            var resultado = await ExecuteQuerySingleStructAsync(query, reader => GetSafeValue<decimal>(reader, 0), parameters, cancellationToken);
            return resultado ?? 0;
        }

        public async Task<decimal> ObterQuantidadeTotalAsync(int veiculoId, string? tipoCombustivel = null, CancellationToken cancellationToken = default)
        {
            var query = @"
                SELECT COALESCE(SUM(Quantidade), 0)
                FROM Abastecimentos
                WHERE VeiculoId = $veiculoId";

            var parameters = new Dictionary<string, object> { { "veiculoId", veiculoId } };

            if (!string.IsNullOrWhiteSpace(tipoCombustivel))
            {
                query += " AND TipoCombustivel = $tipoCombustivel";
                parameters.Add("tipoCombustivel", tipoCombustivel);
            }

            var resultado = await ExecuteQuerySingleStructAsync(query, reader => GetSafeValue<decimal>(reader, 0), parameters, cancellationToken);
            return resultado ?? 0;
        }

        public async Task<decimal> ObterPrecoMedioAsync(int veiculoId, string? tipoCombustivel = null, CancellationToken cancellationToken = default)
        {
            var query = @"
                SELECT COALESCE(AVG(PrecoPorUnidade), 0)
                FROM Abastecimentos
                WHERE VeiculoId = $veiculoId";

            var parameters = new Dictionary<string, object> { { "veiculoId", veiculoId } };

            if (!string.IsNullOrWhiteSpace(tipoCombustivel))
            {
                query += " AND TipoCombustivel = $tipoCombustivel";
                parameters.Add("tipoCombustivel", tipoCombustivel);
            }

            var resultado = await ExecuteQuerySingleStructAsync(query, reader => GetSafeValue<decimal>(reader, 0), parameters, cancellationToken);
            return resultado ?? 0;
        }

        public async Task<int> ContarAbastecimentosAsync(int veiculoId, CancellationToken cancellationToken = default)
        {
            const string query = @"
                SELECT COUNT(*)
                FROM Abastecimentos
                WHERE VeiculoId = $veiculoId";

            var parameters = new Dictionary<string, object> { { "veiculoId", veiculoId } };
            var resultado = await ExecuteQuerySingleStructAsync(query, reader => GetSafeValue<int>(reader, 0), parameters, cancellationToken);
            return resultado ?? 0;
        }

        public async Task<Dictionary<string, int>> ObterEstatisticasPorCombustivelAsync(int veiculoId, CancellationToken cancellationToken = default)
        {
            const string query = @"
                SELECT TipoCombustivel, COUNT(*) as Quantidade
                FROM Abastecimentos
                WHERE VeiculoId = $veiculoId
                GROUP BY TipoCombustivel
                ORDER BY Quantidade DESC";

            var parameters = new Dictionary<string, object> { { "veiculoId", veiculoId } };
            var resultados = await ExecuteQueryAsync(query, reader => new
            {
                TipoCombustivel = GetSafeValue<string>(reader, 0, string.Empty),
                Quantidade = GetSafeValue<int>(reader, 1)
            }, parameters, cancellationToken);

            return resultados.ToDictionary(r => r.TipoCombustivel, r => r.Quantidade);
        }

        public async Task<int> ContarAbastecimentosPorVeiculoAsync(int veiculoId, CancellationToken cancellationToken = default)
        {
            const string query = "SELECT COUNT(*) FROM Abastecimentos WHERE VeiculoId = $veiculoId";
            var parameters = new Dictionary<string, object> { { "veiculoId", veiculoId } };

            var resultado = await ExecuteQuerySingleStructAsync<int>(query, reader => reader.GetInt32(0), parameters, cancellationToken);
            return resultado ?? 0;
        }

        public async Task<decimal> ObterMediaConsumoAsync(int veiculoId, CancellationToken cancellationToken = default)
        {
            const string query = @"
                SELECT AVG(CAST(Litros AS REAL) / CAST(Odometro AS REAL)) as MediaConsumo
                FROM Abastecimentos 
                WHERE VeiculoId = $veiculoId AND Litros > 0 AND Odometro > 0";

            var parameters = new Dictionary<string, object> { { "veiculoId", veiculoId } };

            var resultado = await ExecuteQuerySingleStructAsync<decimal>(query, reader => 
                GetSafeValue<decimal>(reader, "MediaConsumo", 0m), parameters, cancellationToken);
            return resultado ?? 0m;
        }

        public async Task<decimal> ObterTotalGastoPorVeiculoAsync(int veiculoId, CancellationToken cancellationToken = default)
        {
            const string query = "SELECT SUM(ValorTotal) FROM Abastecimentos WHERE VeiculoId = $veiculoId";
            var parameters = new Dictionary<string, object> { { "veiculoId", veiculoId } };

            var resultado = await ExecuteQuerySingleStructAsync<decimal>(query, reader => 
                GetSafeValue<decimal>(reader, 0, 0m), parameters, cancellationToken);
            return resultado ?? 0m;
        }

        public async Task<decimal> ObterTotalLitrosPorVeiculoAsync(int veiculoId, CancellationToken cancellationToken = default)
        {
            const string query = "SELECT SUM(Litros) FROM Abastecimentos WHERE VeiculoId = $veiculoId";
            var parameters = new Dictionary<string, object> { { "veiculoId", veiculoId } };

            var resultado = await ExecuteQuerySingleStructAsync<decimal>(query, reader => 
                GetSafeValue<decimal>(reader, 0, 0m), parameters, cancellationToken);
            return resultado ?? 0m;
        }

        public async Task<int> ContarAbastecimentosAsync(CancellationToken cancellationToken = default)
        {
            const string query = "SELECT COUNT(*) FROM Abastecimentos";
            
            var resultado = await ExecuteQuerySingleStructAsync<int>(query, reader => reader.GetInt32(0), null, cancellationToken);
            return resultado ?? 0;
        }

        // Método de mapeamento privado
        private static Abastecimento MapAbastecimento(IDataReader reader)
        {
            return new Abastecimento
            {
                Id = GetSafeValue<int>(reader, 0),
                VeiculoId = GetSafeValue<int>(reader, 1),
                Data = GetSafeValue<DateTime>(reader, 2),
                TipoCombustivel = GetSafeValue<string>(reader, 3, string.Empty),
                PrecoPorUnidade = GetSafeValue<decimal>(reader, 4),
                Quantidade = GetSafeValue<decimal>(reader, 5),
                QuilometragemAtual = GetSafeValue<int>(reader, 6),
                Posto = GetSafeValue<string>(reader, 7, string.Empty)
                // ValorTotal é calculado automaticamente quando PrecoPorUnidade e Quantidade são definidos
            };
        }
    }
}
