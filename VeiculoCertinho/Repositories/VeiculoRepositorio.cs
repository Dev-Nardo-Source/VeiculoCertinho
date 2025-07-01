using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VeiculoCertinho.Models;
using Microsoft.Data.Sqlite;
using System;
using VeiculoCertinho.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using System.Data;

namespace VeiculoCertinho.Repositories
{
    public class VeiculoRepositorio : BaseRepositorio
    {
        private readonly FirebaseHelper _firebaseHelper;
        private readonly IConfiguration _configuration;

        public VeiculoRepositorio(IConfiguration configuration, ILogger<VeiculoRepositorio>? logger = null)
            : base(logger ?? NullLogger<VeiculoRepositorio>.Instance)
        {
            _configuration = configuration;
            _firebaseHelper = new FirebaseHelper(configuration);
        }

        public async Task<List<Veiculo>> ObterTodosAsync(CancellationToken cancellationToken = default)
        {
            const string query = @"
                SELECT v.Id, v.Marca, v.Modelo, v.AnoFabricacao, v.AnoModelo, v.Placa, 
                       v.UsaGasolina, v.UsaEtanol, v.UsaGNV, v.UsaRecargaEletrica, v.UsaDiesel, v.UsaHidrogenio, 
                       v.MunicipioOrigem, v.UfOrigemId, uf_origem.Sigla as UfOrigem,
                       v.MunicipioAtual, v.UfAtualId, uf_atual.Sigla as UfAtual
                FROM Veiculo v
                LEFT JOIN UF uf_origem ON v.UfOrigemId = uf_origem.Id
                LEFT JOIN UF uf_atual ON v.UfAtualId = uf_atual.Id
                ORDER BY v.Marca ASC";

            return await ExecuteQueryAsync(query, MapVeiculoCompleto, cancellationToken: cancellationToken);
        }

        public async Task<List<Veiculo>> ObterPorUsuarioAsync(string usuarioId, CancellationToken cancellationToken = default)
        {
            const string query = @"
                SELECT v.Id, v.Marca, v.Modelo, v.AnoFabricacao, v.AnoModelo, v.Placa
                FROM Veiculo v
                INNER JOIN UsuariosVeiculos uv ON v.Id = uv.VeiculoId
                WHERE uv.UsuarioId = $usuarioId
                ORDER BY v.Marca ASC";

            var parameters = new Dictionary<string, object> { { "usuarioId", usuarioId } };
            return await ExecuteQueryAsync(query, MapVeiculoBasico, parameters, cancellationToken);
        }

        public async Task<Veiculo?> ObterPorPlacaAsync(string placa, CancellationToken cancellationToken = default)
        {
            const string query = @"
                SELECT Id, Marca, Modelo, AnoFabricacao, AnoModelo, Placa
                FROM Veiculo
                WHERE Placa = $placa";

            var parameters = new Dictionary<string, object> { { "placa", placa.ToUpper().Trim() } };
            return await ExecuteQuerySingleAsync(query, MapVeiculoBasico, parameters, cancellationToken);
        }

        public async Task<Veiculo?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
        {
            const string query = @"
                SELECT v.Id, v.Marca, v.Modelo, v.AnoFabricacao, v.AnoModelo, v.Placa,
                       v.MunicipioOrigem, v.UfOrigemId, uf_origem.Sigla as UfOrigem,
                       v.MunicipioAtual, v.UfAtualId, uf_atual.Sigla as UfAtual
                FROM Veiculo v
                LEFT JOIN UF uf_origem ON v.UfOrigemId = uf_origem.Id
                LEFT JOIN UF uf_atual ON v.UfAtualId = uf_atual.Id
                WHERE v.Id = $id";

            var parameters = new Dictionary<string, object> { { "id", id } };
            return await ExecuteQuerySingleAsync(query, MapVeiculoCompleto, parameters, cancellationToken);
        }

        public async Task<List<VeiculoDetalhes>> ObterDetalhesAsync(CancellationToken cancellationToken = default)
        {
            const string query = @"
                SELECT Id, VeiculoId, Cor, Placa, Foto, Personalizacao, Observacoes
                FROM VeiculoDetalhes
                ORDER BY Id DESC";

            return await ExecuteQueryAsync(query, MapVeiculoDetalhes, cancellationToken: cancellationToken);
        }

        public async Task AdicionarAsync(Veiculo veiculo, CancellationToken cancellationToken = default)
        {
            const string command = @"
                INSERT INTO Veiculo (Marca, Modelo, AnoFabricacao, AnoModelo, Placa, UsaGasolina, UsaEtanol, UsaGNV, UsaRecargaEletrica, UsaDiesel, UsaHidrogenio, MunicipioOrigem, UfOrigemId, MunicipioAtual, UfAtualId)
                VALUES ($marca, $modelo, $anoFabricacao, $anoModelo, $placa, $usaGasolina, $usaEtanol, $usaGNV, $usaRecargaEletrica, $usaDiesel, $usaHidrogenio, $municipioOrigem, $ufOrigemId, $municipioAtual, $ufAtualId)";

            var parameters = new Dictionary<string, object>
            {
                { "marca", veiculo.Marca },
                { "modelo", veiculo.Modelo },
                { "anoFabricacao", veiculo.AnoFabricacao > 0 ? veiculo.AnoFabricacao : 1900 },
                { "anoModelo", veiculo.AnoModelo > 0 ? veiculo.AnoModelo : 1900 },
                { "placa", veiculo.Placa },
                { "usaGasolina", veiculo.UsaGasolina },
                { "usaEtanol", veiculo.UsaEtanol },
                { "usaGNV", veiculo.UsaGNV },
                { "usaRecargaEletrica", veiculo.UsaRecargaEletrica },
                { "usaDiesel", veiculo.UsaDiesel },
                { "usaHidrogenio", veiculo.UsaHidrogenio },
                { "municipioOrigem", veiculo.MunicipioOrigem },
                { "ufOrigemId", veiculo.UfOrigemId },
                { "municipioAtual", veiculo.MunicipioAtual },
                { "ufAtualId", veiculo.UfAtualId }
            };

            await ExecuteCommandAsync(command, parameters, cancellationToken);
        }

        public async Task AtualizarAsync(Veiculo veiculo, CancellationToken cancellationToken = default)
        {
            const string command = @"
                UPDATE Veiculo
                SET Marca = $marca, Modelo = $modelo, AnoFabricacao = $anoFabricacao, AnoModelo = $anoModelo, 
                    Placa = $placa, UsaGasolina = $usaGasolina, UsaEtanol = $usaEtanol, UsaGNV = $usaGNV, 
                    UsaRecargaEletrica = $usaRecargaEletrica, UsaDiesel = $usaDiesel, UsaHidrogenio = $usaHidrogenio, 
                    MunicipioOrigem = $municipioOrigem, UfOrigemId = $ufOrigemId, MunicipioAtual = $municipioAtual, UfAtualId = $ufAtualId
                WHERE Id = $id";

            var parameters = new Dictionary<string, object>
            {
                { "marca", veiculo.Marca },
                { "modelo", veiculo.Modelo },
                { "anoFabricacao", veiculo.AnoFabricacao > 0 ? veiculo.AnoFabricacao : 1900 },
                { "anoModelo", veiculo.AnoModelo > 0 ? veiculo.AnoModelo : 1900 },
                { "placa", veiculo.Placa },
                { "usaGasolina", veiculo.UsaGasolina },
                { "usaEtanol", veiculo.UsaEtanol },
                { "usaGNV", veiculo.UsaGNV },
                { "usaRecargaEletrica", veiculo.UsaRecargaEletrica },
                { "usaDiesel", veiculo.UsaDiesel },
                { "usaHidrogenio", veiculo.UsaHidrogenio },
                { "municipioOrigem", veiculo.MunicipioOrigem },
                { "ufOrigemId", veiculo.UfOrigemId },
                { "municipioAtual", veiculo.MunicipioAtual },
                { "ufAtualId", veiculo.UfAtualId },
                { "id", veiculo.Id }
            };

            await ExecuteCommandAsync(command, parameters, cancellationToken);
        }

        public async Task RemoverAsync(int id, CancellationToken cancellationToken = default)
        {
            const string command = "DELETE FROM Veiculo WHERE Id = $id";
            var parameters = new Dictionary<string, object> { { "id", id } };
            await ExecuteCommandAsync(command, parameters, cancellationToken);
        }

        public async Task AdicionarDetalhesAsync(VeiculoDetalhes detalhes, CancellationToken cancellationToken = default)
        {
            const string command = @"
                INSERT INTO VeiculoDetalhes (VeiculoId, Cor, Placa, Foto, Personalizacao, Observacoes)
                VALUES ($veiculoId, $cor, $placa, $foto, $personalizacao, $observacoes)";

            var parameters = new Dictionary<string, object>
            {
                { "veiculoId", detalhes.VeiculoId },
                { "cor", detalhes.Cor },
                { "placa", detalhes.Placa },
                { "foto", detalhes.Foto },
                { "personalizacao", detalhes.Personalizacao },
                { "observacoes", detalhes.Observacoes }
            };

            await ExecuteCommandAsync(command, parameters, cancellationToken);
        }

        public async Task AtualizarDetalhesAsync(VeiculoDetalhes detalhes, CancellationToken cancellationToken = default)
        {
            const string command = @"
                UPDATE VeiculoDetalhes
                SET VeiculoId = $veiculoId, Cor = $cor, Placa = $placa, Foto = $foto, Personalizacao = $personalizacao, Observacoes = $observacoes
                WHERE Id = $id";

            var parameters = new Dictionary<string, object>
            {
                { "veiculoId", detalhes.VeiculoId },
                { "cor", detalhes.Cor },
                { "placa", detalhes.Placa },
                { "foto", detalhes.Foto },
                { "personalizacao", detalhes.Personalizacao },
                { "observacoes", detalhes.Observacoes },
                { "id", detalhes.Id }
            };

            await ExecuteCommandAsync(command, parameters, cancellationToken);
        }

        // Métodos de mapeamento privados
        private static Veiculo MapVeiculoCompleto(IDataReader reader)
        {
            return new Veiculo
            {
                Id = GetSafeValue<int>(reader, 0),
                Marca = GetSafeValue<string>(reader, 1, string.Empty),
                Modelo = GetSafeValue<string>(reader, 2, string.Empty),
                AnoFabricacao = GetSafeValue<int>(reader, 3),
                AnoModelo = GetSafeValue<int>(reader, 4),
                Placa = GetSafeValue<string>(reader, 5, string.Empty),
                UsaGasolina = GetSafeValue<bool>(reader, 6),
                UsaEtanol = GetSafeValue<bool>(reader, 7),
                UsaGNV = GetSafeValue<bool>(reader, 8),
                UsaRecargaEletrica = GetSafeValue<bool>(reader, 9),
                UsaDiesel = GetSafeValue<bool>(reader, 10),
                UsaHidrogenio = GetSafeValue<bool>(reader, 11),
                MunicipioOrigem = GetSafeValue<string>(reader, 12, string.Empty),
                UfOrigemId = GetSafeValue<int>(reader, 13),
                UfOrigem = GetSafeValue<string>(reader, 14, string.Empty),
                MunicipioAtual = GetSafeValue<string>(reader, 15, string.Empty),
                UfAtualId = GetSafeValue<int>(reader, 16),
                UfAtual = GetSafeValue<string>(reader, 17, string.Empty)
            };
        }

        private static Veiculo MapVeiculoBasico(IDataReader reader)
        {
            return new Veiculo
            {
                Id = GetSafeValue<int>(reader, 0),
                Marca = GetSafeValue<string>(reader, 1, string.Empty),
                Modelo = GetSafeValue<string>(reader, 2, string.Empty),
                AnoFabricacao = GetSafeValue<int>(reader, 3),
                AnoModelo = GetSafeValue<int>(reader, 4),
                Placa = GetSafeValue<string>(reader, 5, string.Empty)
            };
        }

        private static VeiculoDetalhes MapVeiculoDetalhes(IDataReader reader)
        {
            return new VeiculoDetalhes
            {
                Id = GetSafeValue<int>(reader, 0),
                VeiculoId = GetSafeValue<int>(reader, 1),
                Cor = GetSafeValue<string>(reader, 2, string.Empty),
                Placa = GetSafeValue<string>(reader, 3, string.Empty),
                Foto = GetSafeValue<string>(reader, 4, string.Empty),
                Personalizacao = GetSafeValue<string>(reader, 5, string.Empty),
                Observacoes = GetSafeValue<string>(reader, 6, string.Empty)
            };
        }
    }
}
