using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VeiculoCertinho.Models;
using VeiculoCertinho.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace VeiculoCertinho.Services
{
    public class VeiculoService
    {
        private readonly VeiculoRepositorio _veiculoRepositorio;
        private readonly ILogger<VeiculoService> _logger;

        public VeiculoService(IConfiguration configuration, ILogger<VeiculoService>? logger = null)
        {
            _logger = logger ?? NullLogger<VeiculoService>.Instance;
            _veiculoRepositorio = new VeiculoRepositorio(configuration, NullLogger<VeiculoRepositorio>.Instance);
        }

        // Método AdicionarVeiculoAsync removido conforme solicitação

        public async Task<bool> AtualizarVeiculoAsync(Veiculo veiculo)
        {
            try
            {
                _logger.LogInformation("Iniciando atualização de veículo: {Placa}", veiculo.Placa);
                await _veiculoRepositorio.AtualizarAsync(veiculo);
                _logger.LogInformation("Veículo atualizado com sucesso: {Placa}", veiculo.Placa);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar veículo: {Placa}", veiculo.Placa);
                return false;
            }
        }

        public async Task<bool> RemoverVeiculoAsync(int id)
        {
            try
            {
                _logger.LogInformation("Iniciando remoção de veículo: {Id}", id);
                await _veiculoRepositorio.RemoverAsync(id);
                _logger.LogInformation("Veículo removido com sucesso: {Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover veículo: {Id}", id);
                return false;
            }
        }

        public async Task<Veiculo?> ObterVeiculoPorIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Buscando veículo por ID: {Id}", id);
                var veiculo = await _veiculoRepositorio.ObterPorIdAsync(id);
                if (veiculo != null)
                {
                    _logger.LogInformation("Veículo encontrado: {Placa}", veiculo.Placa);
                }
                else
                {
                    _logger.LogWarning("Veículo não encontrado: {Id}", id);
                }
                return veiculo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar veículo por ID: {Id}", id);
                return null;
            }
        }

        public async Task<List<Veiculo>> ObterTodosVeiculosAsync()
        {
            try
            {
                _logger.LogInformation("Buscando todos os veículos");
                var veiculos = await _veiculoRepositorio.ObterTodosAsync();
                _logger.LogInformation("Total de veículos encontrados: {Count}", veiculos.Count);
                return veiculos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar todos os veículos");
                return new List<Veiculo>();
            }
        }

        public async Task<Veiculo?> ObterVeiculoPorPlacaAsync(string placa)
        {
            try
            {
                _logger.LogInformation("Buscando veículo por placa: {Placa}", placa);
                var veiculo = await _veiculoRepositorio.ObterPorPlacaAsync(placa);
                if (veiculo != null)
                {
                    _logger.LogInformation("Veículo encontrado: {Placa}", veiculo.Placa);
                }
                else
                {
                    _logger.LogWarning("Veículo não encontrado: {Placa}", placa);
                }
                return veiculo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar veículo por placa: {Placa}", placa);
                return null;
            }
        }

        public async Task<List<Veiculo>> ObterPorUsuarioAsync(string usuarioId)
        {
            try
            {
                _logger.LogInformation("Buscando veículos do usuário: {UsuarioId}", usuarioId);
                var veiculos = await _veiculoRepositorio.ObterPorUsuarioAsync(usuarioId);
                _logger.LogInformation("Total de veículos encontrados para o usuário: {Count}", veiculos.Count);
                return veiculos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar veículos do usuário: {UsuarioId}", usuarioId);
                return new List<Veiculo>();
            }
        }

        public async Task<bool> CadastrarVeiculoAsync(Veiculo veiculo)
        {
            try
            {
                _logger.LogInformation("Iniciando cadastro de veículo: {Placa}", veiculo.Placa);
                
                // Verificar se o veículo já existe
                var veiculoExistente = await ObterVeiculoPorPlacaAsync(veiculo.Placa);
                if (veiculoExistente != null)
                {
                    _logger.LogWarning("Veículo já cadastrado: {Placa}", veiculo.Placa);
                    return false;
                }

                // Cadastrar o novo veículo diretamente no repositório
                await _veiculoRepositorio.AdicionarAsync(veiculo);
                _logger.LogInformation("Veículo cadastrado com sucesso: {Placa}", veiculo.Placa);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao cadastrar veículo: {Placa}", veiculo.Placa);
                return false;
            }
        }

        public async Task<bool> CorrigirLocalizacaoVeiculoAsync(string placa, string ufAtual, int ufAtualId, string municipioAtual)
        {
            try
            {
                _logger.LogInformation("Iniciando correção de localização do veículo: {Placa}", placa);
                
                var veiculo = await ObterVeiculoPorPlacaAsync(placa);
                if (veiculo == null)
                {
                    _logger.LogWarning("Veículo não encontrado para correção: {Placa}", placa);
                    return false;
                }

                // Atualizar os dados de localização
                veiculo.UfAtual = ufAtual;
                veiculo.UfAtualId = ufAtualId;
                veiculo.MunicipioAtual = municipioAtual;

                _logger.LogInformation("Corrigindo dados: UfAtual={UfAtual}, UfAtualId={UfAtualId}, MunicipioAtual={MunicipioAtual}", 
                    ufAtual, ufAtualId, municipioAtual);

                await _veiculoRepositorio.AtualizarAsync(veiculo);
                _logger.LogInformation("Localização do veículo corrigida com sucesso: {Placa}", placa);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao corrigir localização do veículo: {Placa}", placa);
                return false;
            }
        }
    }
}
