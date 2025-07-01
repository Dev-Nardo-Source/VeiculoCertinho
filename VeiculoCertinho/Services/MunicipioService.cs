using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VeiculoCertinho.Models;

namespace VeiculoCertinho.Services
{
    public class MunicipioService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<MunicipioService> _logger;
        private readonly Dictionary<int, List<Municipio>> _cache = new();
        private readonly object _cacheLock = new();

        public MunicipioService(HttpClient httpClient, ILogger<MunicipioService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<Municipio>> ObterMunicipiosPorUfAsync(int ufId)
        {
            // Verificar cache primeiro
            lock (_cacheLock)
            {
                if (_cache.TryGetValue(ufId, out var municipiosCache))
                {
                    _logger.LogInformation("Municípios da UF {UfId} obtidos do cache ({Count} municípios)", ufId, municipiosCache.Count);
                    return municipiosCache;
                }
            }

            try
            {
                var url = $"https://servicodados.ibge.gov.br/api/v1/localidades/estados/{ufId}/municipios";
                _logger.LogInformation("Buscando municípios da UF {UfId} na API do IBGE", ufId);

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var municipiosIbge = JsonSerializer.Deserialize<List<MunicipioIbge>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                var municipios = new List<Municipio>();
                if (municipiosIbge != null)
                {
                    foreach (var m in municipiosIbge)
                    {
                        municipios.Add(new Municipio
                        {
                            Id = m.Id,
                            Nome = m.Nome,
                            UfId = ufId
                        });
                    }
                }

                // Adicionar ao cache
                lock (_cacheLock)
                {
                    _cache[ufId] = municipios;
                }

                _logger.LogInformation("Encontrados {Count} municípios para UF {UfId} (adicionados ao cache)", municipios.Count, ufId);
                return municipios;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar municípios da UF {UfId}", ufId);
                return new List<Municipio>();
            }
        }

        /// <summary>
        /// Limpa o cache de municípios
        /// </summary>
        public void LimparCache()
        {
            lock (_cacheLock)
            {
                _cache.Clear();
                _logger.LogInformation("Cache de municípios limpo");
            }
        }

        private class MunicipioIbge
        {
            public int Id { get; set; }
            public string Nome { get; set; } = string.Empty;
        }
    }
} 