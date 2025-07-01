using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VeiculoCertinho.Models;
using Microsoft.Data.Sqlite;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace VeiculoCertinho.Repositories
{
    public class UfRepositorio : BaseRepositorio
    {
        private readonly IConfiguration _configuration;

        public UfRepositorio(IConfiguration configuration, ILogger<UfRepositorio>? logger = null)
            : base(logger ?? NullLogger<UfRepositorio>.Instance)
        {
            _configuration = configuration;
        }

        public async Task<List<Uf>> ObterTodosAsync(CancellationToken cancellationToken = default)
        {
            const string query = "SELECT Id, Nome, Sigla FROM UF ORDER BY Sigla ASC";
            return await ExecuteQueryAsync(query, MapUf, cancellationToken: cancellationToken);
        }

        public async Task<Uf?> ObterPorSiglaAsync(string sigla, CancellationToken cancellationToken = default)
        {
            const string query = "SELECT Id, Nome, Sigla FROM UF WHERE Sigla = $sigla";
            var parameters = new Dictionary<string, object> { { "sigla", sigla.ToUpper() } };
            return await ExecuteQuerySingleAsync(query, MapUf, parameters, cancellationToken);
        }

        public async Task<Uf?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
        {
            const string query = "SELECT Id, Nome, Sigla FROM UF WHERE Id = $id";
            var parameters = new Dictionary<string, object> { { "id", id } };
            return await ExecuteQuerySingleAsync(query, MapUf, parameters, cancellationToken);
        }

        public async Task AdicionarAsync(Uf uf, CancellationToken cancellationToken = default)
        {
            const string command = "INSERT INTO UF (Nome, Sigla) VALUES ($nome, $sigla)";
            var parameters = new Dictionary<string, object>
            {
                { "nome", uf.Nome },
                { "sigla", uf.Sigla.ToUpper() }
            };
            await ExecuteCommandAsync(command, parameters, cancellationToken);
        }

        public async Task AtualizarAsync(Uf uf, CancellationToken cancellationToken = default)
        {
            const string command = "UPDATE UF SET Nome = $nome, Sigla = $sigla WHERE Id = $id";
            var parameters = new Dictionary<string, object>
            {
                { "nome", uf.Nome },
                { "sigla", uf.Sigla.ToUpper() },
                { "id", uf.Id }
            };
            await ExecuteCommandAsync(command, parameters, cancellationToken);
        }

        public async Task RemoverAsync(int id, CancellationToken cancellationToken = default)
        {
            const string command = "DELETE FROM UF WHERE Id = $id";
            var parameters = new Dictionary<string, object> { { "id", id } };
            await ExecuteCommandAsync(command, parameters, cancellationToken);
        }

        // Método de mapeamento privado
        private static Uf MapUf(IDataReader reader)
        {
            return new Uf
            {
                Id = GetSafeValue<int>(reader, 0),
                Nome = GetSafeValue<string>(reader, 1, string.Empty),
                Sigla = GetSafeValue<string>(reader, 2, string.Empty)
            };
        }
    }
} 