using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace VeiculoCertinho.Services
{
    public class FirebaseHelper
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<FirebaseHelper>? _logger;
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;
        private readonly string _firebaseUrl;

        public FirebaseHelper(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _firebaseUrl = configuration.GetValue<string>("Firebase:BaseUrl") ?? 
                throw new InvalidOperationException("Firebase:BaseUrl não encontrado na configuração.");
            _apiKey = string.Empty;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        public FirebaseHelper(IConfiguration configuration, ILogger<FirebaseHelper>? logger = null)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger;
            _firebaseUrl = configuration.GetValue<string>("Firebase:BaseUrl") ?? 
                throw new InvalidOperationException("Firebase:BaseUrl não encontrado na configuração.");
            _apiKey = _configuration["FirebaseApiKey"] ?? string.Empty;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<bool> EnviarAsync<T>(string caminho, T objeto, string metodo)
        {
            try
            {
                string url = $"{_firebaseUrl}/{caminho}.json";
                string json = JsonSerializer.Serialize(objeto);
                HttpResponseMessage? response = null;

                switch (metodo.ToUpper())
                {
                    case "POST":
                    case "PUT":
                        var content = new StringContent(json, Encoding.UTF8, "application/json");
                        response = await _httpClient.PutAsync(url, content);
                        break;
                    case "DELETE":
                        response = await _httpClient.DeleteAsync(url);
                        break;
                    default:
                        throw new ArgumentException("Método HTTP inválido para FirebaseHelper");
                }

                if (response == null)
                {
                    _logger?.LogError("Resposta HTTP nula ao enviar dados para Firebase: {Caminho}", caminho);
                    return false;
                }

                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Erro ao enviar dados para Firebase: {Caminho}", caminho);
                return false;
            }
        }

        public async Task<bool> EnviarNotificacaoAsync(string titulo, string mensagem)
        {
            try
            {
                _logger?.LogInformation("Enviando notificação: {Titulo}", titulo);

                if (string.IsNullOrEmpty(_apiKey))
                {
                    _logger?.LogWarning("API Key do Firebase não configurada");
                    return false;
                }

                // Implementar a lógica de envio de notificação aqui
                await Task.Delay(100); // Simulação de envio

                _logger?.LogInformation("Notificação enviada com sucesso: {Titulo}", titulo);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Erro ao enviar notificação: {Titulo}", titulo);
                return false;
            }
        }

        public async Task<string> GetFirebaseTokenAsync()
        {
            try
            {
                var apiKey = _configuration["Firebase:ApiKey"];
                if (string.IsNullOrEmpty(apiKey))
                {
                    _logger?.LogWarning("Firebase API Key não encontrada na configuração.");
                    return string.Empty;
                }

                // Simulação de chamada assíncrona
                await Task.Delay(100);
                return "firebase_token_simulado";
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Erro ao obter token do Firebase");
                return string.Empty;
            }
        }
    }
}
