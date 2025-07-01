using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace VeiculoCertinho.Services
{
    public class DoisFatoresService : BaseService
    {
        private readonly ILogger<DoisFatoresService> _logger;

        public DoisFatoresService(ILogger<DoisFatoresService> logger) : base(logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Gera um código 2FA para o usuário especificado.
        /// </summary>
        /// <param name="usuario">Nome do usuário para o qual o código será gerado.</param>
        /// <returns>Task que retorna o código 2FA gerado.</returns>
        public Task<string> GerarCodigoAsync(string usuario)
        {
            if (string.IsNullOrEmpty(usuario))
                throw new ArgumentException("Usuário não pode ser nulo ou vazio.", nameof(usuario));

            try
            {
                // Implementar geração de código real, envio via SMS ou app autenticador
                var codigo = new Random().Next(100000, 999999).ToString();
                _logger.LogInformation($"Código 2FA gerado para usuário {usuario}.");
                return Task.FromResult(codigo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar código 2FA.");
                throw;
            }
        }

        /// <summary>
        /// Valida o código 2FA recebido comparando com o código gerado.
        /// </summary>
        /// <param name="usuario">Nome do usuário para validação.</param>
        /// <param name="codigoRecebido">Código 2FA recebido para validação.</param>
        /// <param name="codigoGerado">Código 2FA gerado para comparação.</param>
        /// <returns>Task que retorna true se o código for válido, caso contrário false.</returns>
        public Task<bool> ValidarCodigoAsync(string usuario, string codigoRecebido, string codigoGerado)
        {
            if (string.IsNullOrEmpty(usuario))
                throw new ArgumentException("Usuário não pode ser nulo ou vazio.", nameof(usuario));
            if (string.IsNullOrEmpty(codigoRecebido))
                throw new ArgumentException("Código recebido não pode ser nulo ou vazio.", nameof(codigoRecebido));
            if (string.IsNullOrEmpty(codigoGerado))
                throw new ArgumentException("Código gerado não pode ser nulo ou vazio.", nameof(codigoGerado));

            try
            {
                // Implementar validação real comparando código recebido com o gerado
                bool valido = codigoRecebido == codigoGerado;
                _logger.LogInformation($"Validação de código 2FA para usuário {usuario}: {valido}");
                return Task.FromResult(valido);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao validar código 2FA.");
                throw;
            }
        }
    }
}
