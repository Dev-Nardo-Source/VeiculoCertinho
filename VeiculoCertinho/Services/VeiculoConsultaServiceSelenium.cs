using System;
using System.Threading.Tasks;
using VeiculoCertinho.Models;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Text.RegularExpressions;
using OpenQA.Selenium.Interactions;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace VeiculoCertinho.Services
{
    /// <summary>
    /// Serviço otimizado para consulta de veículos via Selenium com Chrome.
    /// Refatorado para eliminar código boilerplate e melhorar performance.
    /// </summary>
    public class VeiculoConsultaServiceSelenium : BaseService, IVeiculoConsultaServiceSelenium
    {
        #region Constantes e Configurações

        private const string BASE_URL = "https://www.keplaca.com/placa?placa-fipe=";
        private const string CSS_SELECTOR_TABELA = "table.fipeTablePriceDetail";
        private const int DELAY_MIN_MS = 2000;
        private const int DELAY_MAX_MS = 4000;
        private const int RETRY_DELAY_MIN_MS = 3000;
        private const int RETRY_DELAY_MAX_MS = 8000;

        private static readonly Random _random = new();
        
        private static readonly string[] CHROME_STEALTH_SCRIPTS = {
            "Object.defineProperty(navigator, 'webdriver', {get: () => undefined})",
            "Object.defineProperty(navigator, 'plugins', {get: () => [1, 2, 3, 4, 5]})",
            "Object.defineProperty(navigator, 'languages', {get: () => ['pt-BR', 'pt', 'en-US', 'en']})",
            "window.chrome = { runtime: {} };"
        };

        private static readonly string[] VALID_KEYS = { 
            "Placa:", "Chassi:", "Marca:", "Modelo:", "Importado:", "Ano:", "Ano Modelo:",
            "Cor:", "Cilindrada:", "Potencia:", "Combustível:", "Motor:", "Passageiros:", 
            "UF:", "Município:", "Segmento:", "Especie Veiculo:"
        };

        private static readonly Dictionary<string, int> UF_MAPPING = new(StringComparer.OrdinalIgnoreCase) {
            { "RO", 11 }, { "AC", 12 }, { "AM", 13 }, { "RR", 14 }, { "PA", 15 }, { "AP", 16 }, { "TO", 17 },
            { "MA", 21 }, { "PI", 22 }, { "CE", 23 }, { "RN", 24 }, { "PB", 25 }, { "PE", 26 }, { "AL", 27 }, 
            { "SE", 28 }, { "BA", 29 }, { "MG", 31 }, { "ES", 32 }, { "RJ", 33 }, { "SP", 35 }, { "PR", 41 }, 
            { "SC", 42 }, { "RS", 43 }, { "MS", 50 }, { "MT", 51 }, { "GO", 52 }, { "DF", 53 }
        };

        #endregion

        #region Propriedades

        private readonly int _timeoutSeconds;
        private readonly int _retryCount;

        #endregion

        #region Construtor

        public VeiculoConsultaServiceSelenium(ILogger<VeiculoConsultaServiceSelenium> logger, int timeoutSeconds = 30, int retryCount = 3) 
            : base(logger)
        {
            _timeoutSeconds = timeoutSeconds;
            _retryCount = retryCount;
            _logger.LogInformation("Serviço Selenium inicializado: timeout={TimeoutSeconds}s, retry={RetryCount}", timeoutSeconds, retryCount);
        }

        #endregion

        #region Método Principal

        public async Task<Veiculo?> ConsultarVeiculoPorPlacaAsync(string placa, CancellationToken cancellationToken = default)
        {
            ValidateNotNullOrEmpty(placa, nameof(placa));
            var placaFormatada = placa.ToUpper().Trim();

            return await ExecuteWithRetryAsync(async () =>
            {
                LogCrudOperation("ConsultarVeiculo", "Veiculo", placaFormatada);
                
                using var driver = CreateOptimizedChromeDriver();
                ConfigureDriver(driver);
                ExecuteStealthScripts(driver);

                var detalhes = await ExtractVehicleDetailsAsync(driver, placaFormatada, cancellationToken);
                var veiculo = CreateVehicleFromDetails(detalhes, placaFormatada);

                LogCrudSuccess("ConsultarVeiculo", "Veiculo", placaFormatada);
                return veiculo;

            }, "Consultar veículo por placa", _retryCount, cancellationToken: cancellationToken);
        }

        #endregion

        #region Métodos de Configuração

        private static ChromeDriver CreateOptimizedChromeDriver()
        {
            var options = new ChromeOptions();
            
            // Configurações core para performance
            options.AddArguments(
                "--headless=new",
                "--no-sandbox", 
                "--disable-dev-shm-usage",
                "--disable-gpu",
                "--disable-extensions",
                "--disable-notifications",
                "--disable-popup-blocking",
                "--disable-infobars",
                "--window-size=1920,1080"
            );

            // Stealth mode
            options.AddArguments(
                "--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
                "--disable-blink-features=AutomationControlled"
            );
            options.AddExcludedArgument("enable-automation");
            options.AddAdditionalChromeOption("useAutomationExtension", false);

            // Preferências otimizadas
            var prefs = new Dictionary<string, object>
            {
                ["credentials_enable_service"] = false,
                ["profile.password_manager_enabled"] = false,
                ["profile.default_content_setting_values.notifications"] = 2,
                ["profile.managed_default_content_settings.images"] = 1,
                ["profile.default_content_settings.popups"] = 0,
                ["download.prompt_for_download"] = false
            };
            options.AddUserProfilePreferences(prefs);

            return new ChromeDriver(options);
        }

        private void ConfigureDriver(ChromeDriver driver)
        {
            var timeouts = driver.Manage().Timeouts();
            timeouts.PageLoad = TimeSpan.FromSeconds(_timeoutSeconds);
            timeouts.ImplicitWait = TimeSpan.FromSeconds(5);
        }

        private static void ExecuteStealthScripts(IJavaScriptExecutor driver)
        {
            foreach (var script in CHROME_STEALTH_SCRIPTS)
            {
                driver.ExecuteScript(script);
            }
        }

        #endregion

        #region Extração de Dados

        private async Task<Dictionary<string, string>> ExtractVehicleDetailsAsync(ChromeDriver driver, string placa, CancellationToken cancellationToken)
        {
            var url = BASE_URL + placa;
            driver.Navigate().GoToUrl(url);
            
            await Task.Delay(_random.Next(DELAY_MIN_MS, DELAY_MAX_MS), cancellationToken);

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(_timeoutSeconds));
            
            return wait.Until(d => ExtractDetailsFromPage(d));
        }

        private Dictionary<string, string> ExtractDetailsFromPage(IWebDriver driver)
        {
            ValidatePageContent(driver);
            
            var tabela = driver.FindElement(By.CssSelector(CSS_SELECTOR_TABELA));
            var linhas = tabela.FindElements(By.TagName("tr"));
            var detalhes = new Dictionary<string, string>();

            foreach (var linha in linhas)
            {
                var colunas = linha.FindElements(By.TagName("td"));
                if (colunas.Count != 2) continue;

                var (chave, valor) = ProcessTableRow(colunas[0].Text.Trim(), colunas[1].Text.Trim());
                if (!string.IsNullOrEmpty(chave) && IsValidKey(chave))
                {
                    detalhes[chave] = valor;
                }
            }

            return detalhes;
        }

        private static void ValidatePageContent(IWebDriver driver)
        {
            var body = driver.FindElement(By.TagName("body"));
            var textoCompleto = body.Text.ToLower();
            
            if (textoCompleto.Contains("captcha") || textoCompleto.Contains("verificação") || textoCompleto.Contains("robô"))
            {
                throw new InvalidOperationException("Verificação de captcha detectada");
            }
        }

        private static (string chave, string valor) ProcessTableRow(string chaveOriginal, string valorOriginal)
        {
            // Normalizar chave
            var chave = Regex.Replace(chaveOriginal, @"\s+", " ");
            var indexDoisPontos = chave.IndexOf(':');
            if (indexDoisPontos > 0)
            {
                chave = chave.Substring(0, indexDoisPontos).Trim() + ":";
            }

            // Normalizar valor
            var valor = valorOriginal;
            var indexIgual = valor.IndexOf('=');
            if (indexIgual >= 0 && indexIgual < valor.Length - 1)
            {
                valor = valor.Substring(indexIgual + 1).Trim();
            }

            // Processamento específico por tipo de chave
            valor = ProcessValueByKey(chave, valor);

            return (chave, valor);
        }

        private static string ProcessValueByKey(string chave, string valor)
        {
            return chave switch
            {
                "Segmento:" or "Importado:" or "Cor:" => GetFirstWord(valor),
                "UF:" => valor.ToUpper().Length > 2 ? valor.ToUpper().Substring(0, 2) : valor.ToUpper(),
                "Ano:" or "Ano Modelo:" or "Passageiros:" => Regex.Replace(valor, @"\D", ""),
                _ => valor
            };
        }

        private static string GetFirstWord(string input)
        {
            var partes = Regex.Split(input.Trim(), @"(?<!^)(?=[A-Z])");
            return partes.Length > 0 ? partes[0] : input;
        }

        private static bool IsValidKey(string chave)
        {
            return VALID_KEYS.Any(vk => string.Equals(chave, vk, StringComparison.OrdinalIgnoreCase));
        }

        #endregion

        #region Criação do Modelo

        private static Veiculo CreateVehicleFromDetails(Dictionary<string, string> detalhes, string placa)
        {
            var ufOrigem = detalhes.GetValueOrDefault("UF:", "");
            var ufOrigemId = GetUfIdBySigla(ufOrigem);
            
            var veiculo = new Veiculo
            {
                Placa = placa,
                Marca = GetValueOrDefault(detalhes, "Marca:"),
                Modelo = GetValueOrDefault(detalhes, "Modelo:"),
                AnoFabricacao = ParseAno(detalhes.GetValueOrDefault("Ano:", "0")),
                AnoModelo = ParseAno(detalhes.GetValueOrDefault("Ano Modelo:", "0")),
                Cor = GetValueOrDefault(detalhes, "Cor:"),
                Cilindrada = GetValueOrDefault(detalhes, "Cilindrada:"),
                Potencia = GetValueOrDefault(detalhes, "Potencia:"),
                Passageiros = int.TryParse(detalhes.GetValueOrDefault("Passageiros:", "0"), out int passageiros) ? passageiros : 0,
                UfOrigem = ufOrigem,
                UfOrigemId = ufOrigemId,
                UfAtual = ufOrigem,
                UfAtualId = ufOrigemId,
                MunicipioOrigem = GetValueOrDefault(detalhes, "Município:"),
                MunicipioAtual = GetValueOrDefault(detalhes, "Município:"),
                Segmento = GetValueOrDefault(detalhes, "Segmento:"),
                EspecieVeiculo = GetValueOrDefault(detalhes, "Especie Veiculo:"),
                Chassi = detalhes.GetValueOrDefault("Chassi:", ""),
                Motor = GetValueOrDefault(detalhes, "Motor:"),
                Importado = detalhes.GetValueOrDefault("Importado:", "").ToLower() == "sim",
                Observacoes = detalhes.GetValueOrDefault("Observações:", "")
            };

            ConfigureFuelTypes(veiculo, detalhes.GetValueOrDefault("Combustível:", ""));
            return veiculo;
        }

        private static void ConfigureFuelTypes(Veiculo veiculo, string combustiveis)
        {
            var combustiveisArray = combustiveis.ToLower().Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            
            // Reset all fuel types
            veiculo.UsaGasolina = veiculo.UsaEtanol = veiculo.UsaGNV = veiculo.UsaRecargaEletrica = veiculo.UsaDiesel = veiculo.UsaHidrogenio = false;

            foreach (var combustivel in combustiveisArray.Select(c => c.Trim()))
            {
                if (combustivel.Contains("gasolina")) veiculo.UsaGasolina = true;
                if (combustivel.Contains("etanol") || combustivel.Contains("álcool")) veiculo.UsaEtanol = true;
                if (combustivel.Contains("gnv") || combustivel.Contains("gás natural")) veiculo.UsaGNV = true;
                if (combustivel.Contains("elétric") || combustivel.Contains("eletric")) veiculo.UsaRecargaEletrica = true;
                if (combustivel.Contains("diesel") || combustivel.Contains("díesel")) veiculo.UsaDiesel = true;
                if (combustivel.Contains("hidrogênio") || combustivel.Contains("hidrogenio")) veiculo.UsaHidrogenio = true;
            }
        }

        #endregion

        #region Métodos Auxiliares

        private static string GetValueOrDefault(Dictionary<string, string> detalhes, string chave)
        {
            var valor = detalhes.GetValueOrDefault(chave, "");
            return string.IsNullOrWhiteSpace(valor) ? "Não Localizado" : valor;
        }

        private static int ParseAno(string anoStr)
        {
            return int.TryParse(anoStr, out int ano) && ano >= 1900 && ano <= 2100 ? ano : 1900;
        }

        private static int GetUfIdBySigla(string sigla)
        {
            return UF_MAPPING.TryGetValue(sigla?.Trim() ?? string.Empty, out int id) ? id : 33; // Default RJ
        }

        #endregion
    }
}