using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace VeiculoCertinho.Utils
{
    /// <summary>
    /// Helper para envio de e-mails.
    /// </summary>
    public class EmailHelper
    {
        private readonly ILogger<EmailHelper> _logger;
        private readonly SmtpClient _smtpClient;
        private readonly string _remetente;

        public EmailHelper(ILogger<EmailHelper> logger, string smtpHost, int smtpPort, string remetente, string? smtpUser = null, string? smtpPass = null)
        {
            _logger = logger;
            _remetente = remetente;
            _smtpClient = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = true
            };

            if (!string.IsNullOrEmpty(smtpUser) && !string.IsNullOrEmpty(smtpPass))
            {
                _smtpClient.Credentials = new NetworkCredential(smtpUser, smtpPass);
            }
        }

        /// <summary>
        /// Envia um e-mail de forma assíncrona.
        /// </summary>
        /// <param name="destinatario">Endereço de e-mail do destinatário.</param>
        /// <param name="assunto">Assunto do e-mail.</param>
        /// <param name="corpo">Corpo do e-mail.</param>
        /// <returns>Task representando a operação assíncrona.</returns>
        public async Task EnviarEmailAsync(string destinatario, string assunto, string corpo)
        {
            try
            {
                var mailMessage = new MailMessage(_remetente, destinatario, assunto, corpo);
                await _smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation($"E-mail enviado para {destinatario} com assunto '{assunto}'.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao enviar e-mail para {destinatario}.");
                throw;
            }
        }
    }
}
