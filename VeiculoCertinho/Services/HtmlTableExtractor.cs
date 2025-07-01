using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using VeiculoCertinho.Models;
using System.Diagnostics;
using System.Linq;

namespace VeiculoCertinho.Services
{
    public static class HtmlTableExtractor
    {
        private static readonly Dictionary<string, string> _fieldMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "marca", "marca" },
            { "modelo", "modelo" },
            { "importado", "importado" },
            { "ano", "ano" },
            { "anomodelo", "anoModelo" },
            { "ano modelo", "anoModelo" },
            { "cor", "cor" },
            { "cilindrada", "cilindrada" },
            { "motor", "motor" },
            { "potencia", "potencia" },
            { "potência", "potencia" },
            { "combustivel", "combustivel" },
            { "combustível", "combustivel" },
            { "chassi", "chassi" },
            { "passageiros", "passageiros" },
            { "uf", "ufOrigem" },
            { "municipio", "municipioOrigem" },
            { "município", "municipioOrigem" },
            { "segmento", "segmento" },
            { "especieveiculo", "especieVeiculo" },
            { "especie veiculo", "especieVeiculo" },
            { "espécie veículo", "especieVeiculo" },
            { "especie", "especieVeiculo" }
        };

        public static Veiculo ExtractVehicleInfo(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                throw new ArgumentException("O HTML fornecido está vazio ou nulo.");
            }

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            Debug.WriteLine($"HTML carregado: {doc.DocumentNode.OuterHtml.Length} caracteres.");
            
            var vehicleInfo = new Veiculo();
            
            // Inicializar com valores válidos para evitar constraint violations
            vehicleInfo.UfOrigemId = 35; // SP como padrão
            vehicleInfo.UfAtualId = 35; // SP como padrão
            
            // Buscar todas as tabelas do documento
            var tables = doc.DocumentNode.SelectNodes("//table");
            
            if (tables == null || tables.Count == 0)
            {
                Debug.WriteLine("Nenhuma tabela encontrada no HTML.");
                
                // Tentar encontrar dados em divs estruturadas
                var divs = doc.DocumentNode.SelectNodes("//div[contains(@class, 'vehicle-info') or contains(@class, 'dados')]");
                if (divs != null && divs.Count > 0)
                {
                    Debug.WriteLine($"Encontradas {divs.Count} divs com possíveis dados do veículo");
                    ExtractDataFromDivs(divs!, vehicleInfo);
                }
                else
                {
                    throw new InvalidOperationException("Nenhuma tabela ou div com dados encontrada no HTML");
                }
            }
            else
            {
                Debug.WriteLine($"Encontradas {tables.Count} tabelas no HTML");
                
                // Tentativa 1: Procurar tabela com rótulo 'Marca'
                HtmlNode? targetTable = FindTableWithLabel(tables, "marca");
                
                // Tentativa 2: Se não encontrou, tentar encontrar tabela com estrutura de dados de veículo
                if (targetTable == null)
                {
                    targetTable = FindMostLikelyVehicleTable(tables);
                }
                
                if (targetTable == null)
                {
                    Debug.WriteLine("Tabela de dados do veículo não encontrada");
                    throw new InvalidOperationException("Tabela de dados do veículo não encontrada");
                }
                
                // Extrair dados da tabela encontrada
                ExtractDataFromTable(targetTable, vehicleInfo);
            }
            
            // Verificação se extração foi bem sucedida
            if (string.IsNullOrWhiteSpace(vehicleInfo.Marca) && string.IsNullOrWhiteSpace(vehicleInfo.Modelo))
            {
                Debug.WriteLine("Dados básicos do veículo (marca/modelo) não foram encontrados");
                
                // Última tentativa: buscar em qualquer elemento que contenha texto relevante
                var allElements = doc.DocumentNode.SelectNodes("//*[text()]");
                if (allElements != null)
                {
                    foreach (var element in allElements)
                    {
                        var text = element.InnerText.Trim().ToLowerInvariant();
                        foreach (var mapping in _fieldMappings)
                        {
                            if (text.Contains(mapping.Key))
                            {
                                var value = ExtractValueFromText(text);
                                if (!string.IsNullOrWhiteSpace(value))
                                {
                                    SetVehicleProperty(vehicleInfo, mapping.Value, value);
                                }
                            }
                        }
                    }
                }
                
                if (string.IsNullOrWhiteSpace(vehicleInfo.Marca) && string.IsNullOrWhiteSpace(vehicleInfo.Modelo))
                {
                    throw new InvalidOperationException("Não foi possível extrair dados básicos do veículo");
                }
            }
            
            return vehicleInfo;
        }
        
        private static string ExtractValueFromText(string text)
        {
            var parts = text.Split(new[] { ':', '-', '=' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                return parts[1].Trim();
            }
            return string.Empty;
        }
        
        private static void ExtractDataFromDivs(HtmlNodeCollection divs, Veiculo vehicleInfo)
        {
            foreach (var div in divs)
            {
                var labels = div.SelectNodes(".//label | .//span[contains(@class, 'label')]");
                if (labels != null)
                {
                    foreach (var label in labels)
                    {
                        var labelText = HtmlEntity.DeEntitize(label.InnerText)?.Trim().Replace(":", "") ?? string.Empty;
                        var valueNode = label.NextSibling;
                        
                        if (valueNode != null)
                        {
                            var value = HtmlEntity.DeEntitize(valueNode.InnerText)?.Trim() ?? string.Empty;
                            var normalizedLabel = labelText.Replace(" ", "").ToLowerInvariant();
                            
                            if (_fieldMappings.TryGetValue(normalizedLabel, out string? propertyName))
                            {
                                SetVehicleProperty(vehicleInfo, propertyName, value);
                            }
                        }
                    }
                }
            }
        }
        
        private static HtmlNode? FindTableWithLabel(HtmlNodeCollection tables, string labelToFind)
        {
            foreach (var table in tables)
            {
                var rows = table.SelectNodes(".//tr");
                if (rows == null) continue;
                
                foreach (var row in rows)
                {
                    var cells = row.SelectNodes(".//td | .//th");
                    if (cells != null && cells.Count >= 2 && cells[0] != null)
                    {
                        var label = HtmlEntity.DeEntitize(cells[0].InnerText)?.Trim().Replace(":", "").Replace(" ", "").ToLowerInvariant() ?? string.Empty;
                        if (label == labelToFind)
                        {
                            Debug.WriteLine($"Tabela encontrada contendo label '{labelToFind}'");
                            return table;
                        }
                    }
                }
            }
            
            Debug.WriteLine($"Nenhuma tabela encontrada com label '{labelToFind}'");
            return null;
        }
        
        private static HtmlNode? FindMostLikelyVehicleTable(HtmlNodeCollection tables)
        {
            HtmlNode? bestMatch = null;
            int highestScore = 0;
            
            // Palavras-chave que indicam dados de veículo
            string[] vehicleKeywords = { "marca", "modelo", "ano", "cor", "placa", "combustivel", "fabricante", "montadora" };
            
            foreach (var table in tables)
            {
                int score = 0;
                var tableText = table.InnerText.ToLowerInvariant();
                
                foreach (var keyword in vehicleKeywords)
                {
                    if (tableText.Contains(keyword))
                    {
                        score++;
                    }
                }
                
                if (score > highestScore)
                {
                    highestScore = score;
                    bestMatch = table;
                }
            }
            
            Debug.WriteLine($"Busca heurística de tabela: melhor pontuação = {highestScore}");
            return bestMatch;
        }
        
        private static void ExtractDataFromTable(HtmlNode table, Veiculo vehicleInfo)
        {
            var rows = table.SelectNodes(".//tr");
            if (rows == null)
            {
                Debug.WriteLine("Nenhuma linha encontrada na tabela selecionada");
                return;
            }
            
            Debug.WriteLine($"Processando {rows.Count} linhas da tabela");
            
            // Primeiro, tentar encontrar tabela específica com classe fipeTablePriceDetail
            var detailTable = table.SelectSingleNode("//table[contains(@class, 'fipeTablePriceDetail')]");
            if (detailTable != null)
            {
                Debug.WriteLine("Tabela fipeTablePriceDetail encontrada");
                rows = detailTable.SelectNodes(".//tr");
                if (rows == null)
                {
                    Debug.WriteLine("Nenhuma linha encontrada na tabela fipeTablePriceDetail");
                    return;
                }
            }
            
            foreach (var row in rows)
            {
                if (row == null)
                {
                    continue;
                }
                var cells = row.SelectNodes(".//td | .//th");
                if (cells == null || cells.Count < 2)
                {
                    // Tentar formato "Campo = Valor" em uma única célula
                    if (cells != null && cells.Count == 1)
                    {
                        var text = HtmlEntity.DeEntitize(cells[0].InnerText)?.Trim() ?? string.Empty;
                        if (text.Contains("="))
                        {
                            var parts = text.Split('=');
                            if (parts.Length == 2)
                            {
                                string rotuloCampo = parts[0].Trim();
                                string valorCampo = parts[1].Trim();
                                ProcessFieldValue(vehicleInfo, rotuloCampo, valorCampo);
                            }
                        }
                    }
                    continue;
                }

                string label = HtmlEntity.DeEntitize(cells[0].InnerText)?.Trim().Replace(":", "") ?? string.Empty;
                string value = HtmlEntity.DeEntitize(cells[1].InnerText)?.Trim() ?? string.Empty;

                ProcessFieldValue(vehicleInfo, label, value);
            }
        }
        
        private static void ProcessFieldValue(Veiculo vehicleInfo, string label, string value)
        {
            // Limpar e normalizar o rótulo para correspondência
            string normalizedLabel = label.Replace(" ", "").ToLowerInvariant();
            
            Debug.WriteLine($"Detalhe encontrado: {label} = {value}");
            
            // Tentar encontrar um mapeamento para este rótulo
            if (_fieldMappings.TryGetValue(normalizedLabel, out string? mappedPropertyName))
            {
                SetVehicleProperty(vehicleInfo, mappedPropertyName, value);
            }
            else if (_fieldMappings.TryGetValue(label.ToLowerInvariant(), out mappedPropertyName))
            {
                SetVehicleProperty(vehicleInfo, mappedPropertyName, value);
            }
            else
            {
                Debug.WriteLine($"Nenhum mapeamento encontrado para o rótulo: '{normalizedLabel}'");
            }
        }
        
        private static void SetVehicleProperty(Veiculo veiculo, string propertyName, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }
            
            try
            {
                switch (propertyName.ToLowerInvariant())
                {
                    case "marca":
                        veiculo.Marca = value;
                        Debug.WriteLine($"Marca definida: {value}");
                        break;
                    case "modelo":
                        veiculo.Modelo = value;
                        Debug.WriteLine($"Modelo definido: {value}");
                        break;
                    case "importado":
                        veiculo.Importado = value.ToLower().Contains("sim");
                        Debug.WriteLine($"Importado definido: {value}");
                        break;
                    case "ano":
                        if (int.TryParse(value.Split('/')[0], out int ano))
                        {
                            veiculo.AnoFabricacao = ano;
                            Debug.WriteLine($"Ano definido: {ano}");
                        }
                        break;
                    case "anomodelo":
                        if (int.TryParse(value.Split('/')[0], out int anoModelo))
                        {
                            veiculo.AnoModelo = anoModelo;
                            Debug.WriteLine($"Ano Modelo definido: {anoModelo}");
                        }
                        break;
                    case "cor":
                        veiculo.Cor = value;
                        Debug.WriteLine($"Cor definida: {value}");
                        break;
                    case "cilindrada":
                        veiculo.Cilindrada = value;
                        Debug.WriteLine($"Cilindrada definida: {value}");
                        break;
                    case "potencia":
                        veiculo.Potencia = value;
                        Debug.WriteLine($"Potência definida: {value}");
                        break;
                    case "combustivel":
                        // Removido o uso da propriedade Combustiveis, substituindo por propriedades booleanas
                        var combustiveis = value.Split(new char[] { ',', ';' }, System.StringSplitOptions.RemoveEmptyEntries)
                                                .Select(c => c.Trim())
                                                .Where(c => !string.IsNullOrEmpty(c))
                                                .ToList();

                        veiculo.UsaGasolina = combustiveis.Contains("Gasolina");
                        veiculo.UsaEtanol = combustiveis.Contains("Etanol");
                        veiculo.UsaGNV = combustiveis.Contains("GNV");
                        veiculo.UsaRecargaEletrica = combustiveis.Contains("Recarga Elétrica");
                        veiculo.UsaDiesel = combustiveis.Contains("Diesel");
                        veiculo.UsaHidrogenio = combustiveis.Contains("Hidrogênio");

                        Debug.WriteLine($"Combustíveis definidos: {string.Join(", ", combustiveis)}");
                        break;
                    case "passageiros":
                        if (int.TryParse(value, out int passageiros))
                        {
                            veiculo.Passageiros = passageiros;
                            Debug.WriteLine($"Número de passageiros definido: {passageiros}");
                        }
                        break;
                    case "uf":
                        veiculo.UfOrigem = value;
                        // Mapear sigla para ID (baseado na tabela UF)
                        var ufId = GetUfIdBySigla(value);
                        if (ufId > 0)
                        {
                            veiculo.UfOrigemId = ufId;
                            veiculo.UfAtual = value;
                            veiculo.UfAtualId = ufId;
                        }
                        Debug.WriteLine($"UFOrigem definida: {value} (ID: {ufId})");
                        break;
                    case "municipio":
                        veiculo.MunicipioOrigem = value;
                        Debug.WriteLine($"MunicipioOrigem definido: {value}");
                        break;
                    case "segmento":
                        veiculo.Segmento = value;
                        Debug.WriteLine($"Segmento definido: {value}");
                        break;
                    case "especieveiculo":
                        veiculo.EspecieVeiculo = value;
                        Debug.WriteLine($"Espécie do veículo definida: {value}");
                        break;
                    case "chassi":
                        veiculo.Chassi = value;
                        Debug.WriteLine($"Chassi definido: {value}");
                        break;
                    case "motor":
                        veiculo.Motor = value;
                        Debug.WriteLine($"Motor definido: {value}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao definir propriedade {propertyName}: {ex.Message}");
            }
        }

        private static int GetUfIdBySigla(string sigla)
        {
            // Mapear siglas das UFs para seus IDs baseado na tabela UF
            var ufMapping = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                { "RO", 11 }, { "AC", 12 }, { "AM", 13 }, { "RR", 14 }, { "PA", 15 },
                { "AP", 16 }, { "TO", 17 }, { "MA", 21 }, { "PI", 22 }, { "CE", 23 },
                { "RN", 24 }, { "PB", 25 }, { "PE", 26 }, { "AL", 27 }, { "SE", 28 },
                { "BA", 29 }, { "MG", 31 }, { "ES", 32 }, { "RJ", 33 }, { "SP", 35 },
                { "PR", 41 }, { "SC", 42 }, { "RS", 43 }, { "MS", 50 }, { "MT", 51 },
                { "GO", 52 }, { "DF", 53 }
            };

            return ufMapping.TryGetValue(sigla?.Trim() ?? string.Empty, out int id) ? id : 35; // Default para SP
        }
    }
}
