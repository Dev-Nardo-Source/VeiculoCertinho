using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VeiculoCertinho.Utils;

namespace VeiculoCertinho.Services
{
    /// <summary>
    /// Serviço de integração modernizado usando CommunicationHelper consolidado.
    /// </summary>
    public class IntegracaoService : BaseService
    {
        private readonly EmailHelper _emailHelper;
        private readonly ILogger<IntegracaoService> _logger;

        public IntegracaoService(EmailHelper emailHelper, ILogger<IntegracaoService> logger) : base(logger)
        {
            _emailHelper = emailHelper ?? throw new ArgumentNullException(nameof(emailHelper));
            _logger = logger;
        }

        /// <summary>
        /// Envia um e-mail de forma assíncrona.
        /// </summary>
        /// <param name="destinatario">E-mail do destinatário.</param>
        /// <param name="assunto">Assunto do e-mail.</param>
        /// <param name="corpo">Corpo do e-mail.</param>
        /// <returns>Task representando a operação assíncrona.</returns>
        public async Task EnviarEmailAsync(string destinatario, string assunto, string corpo)
        {
            if (string.IsNullOrEmpty(destinatario))
                throw new ArgumentException("Destinatário não pode ser nulo ou vazio.", nameof(destinatario));
            if (string.IsNullOrEmpty(assunto))
                throw new ArgumentException("Assunto não pode ser nulo ou vazio.", nameof(assunto));
            if (string.IsNullOrEmpty(corpo))
                throw new ArgumentException("Corpo do e-mail não pode ser nulo ou vazio.", nameof(corpo));

            await ExecuteWithErrorHandlingAsync(async () =>
            {
                await _emailHelper.EnviarEmailAsync(destinatario, assunto, corpo);
                _logger.LogInformation($"E-mail enviado para {destinatario}: {assunto}");
            }, "EnviarEmailAsync");
        }

        /// <summary>
        /// Envia um SMS usando o CommunicationHelper consolidado.
        /// </summary>
        /// <param name="numero">Número de telefone do destinatário.</param>
        /// <param name="mensagem">Mensagem do SMS.</param>
        /// <returns>Task representando a operação assíncrona.</returns>
        public async Task EnviarSmsAsync(string numero, string mensagem)
        {
            if (string.IsNullOrEmpty(numero))
                throw new ArgumentException("Número não pode ser nulo ou vazio.", nameof(numero));
            if (string.IsNullOrEmpty(mensagem))
                throw new ArgumentException("Mensagem não pode ser nula ou vazia.", nameof(mensagem));

            await ExecuteWithErrorHandlingAsync(async () =>
            {
                await CommunicationHelper.EnviarSmsAsync(numero, mensagem, _logger);
                _logger.LogInformation($"SMS enviado para {numero}: {mensagem}");
            }, "EnviarSmsAsync");
        }

        /// <summary>
        /// Envia uma notificação usando o CommunicationHelper consolidado.
        /// </summary>
        /// <param name="titulo">Título da notificação.</param>
        /// <param name="mensagem">Mensagem da notificação.</param>
        /// <returns>Task representando a operação assíncrona.</returns>
        public async Task EnviarNotificacaoAsync(string titulo, string mensagem)
        {
            if (string.IsNullOrEmpty(titulo))
                throw new ArgumentException("Título não pode ser nulo ou vazio.", nameof(titulo));
            if (string.IsNullOrEmpty(mensagem))
                throw new ArgumentException("Mensagem não pode ser nula ou vazia.", nameof(mensagem));

            await ExecuteWithErrorHandlingAsync(async () =>
            {
                await CommunicationHelper.EnviarNotificacaoAsync(titulo, mensagem, _logger);
                _logger.LogInformation($"Notificação enviada: {titulo} - {mensagem}");
            }, "EnviarNotificacaoAsync");
        }

        /// <summary>
        /// Valida um número de telefone brasileiro usando CommunicationHelper.
        /// </summary>
        /// <param name="numero">Número para validar.</param>
        /// <returns>True se válido.</returns>
        public bool ValidarTelefone(string numero)
        {
            return CommunicationHelper.IsValidPhoneNumber(numero);
        }

        /// <summary>
        /// Formata um número de telefone usando CommunicationHelper.
        /// </summary>
        /// <param name="numero">Número para formatar.</param>
        /// <returns>Número formatado.</returns>
        public string FormatarTelefone(string numero)
        {
            return CommunicationHelper.FormatPhoneNumber(numero);
        }
    }
}
