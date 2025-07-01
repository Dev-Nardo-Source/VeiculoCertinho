using System.Threading.Tasks;
using VeiculoCertinho.Security;

namespace VeiculoCertinho.Services
{
    public class SegurancaService
    {
        private readonly Seguranca _seguranca;

        public SegurancaService(string secretKey, Microsoft.Extensions.Logging.ILogger<Seguranca> logger)
        {
            _seguranca = new Seguranca(secretKey, logger);
        }

        /// <summary>
        /// Gera um token JWT para o usuário especificado.
        /// </summary>
        public async Task<string> GerarTokenAsync(string usuario)
        {
            if (string.IsNullOrEmpty(usuario))
                throw new System.ArgumentException("Usuário não pode ser nulo ou vazio.", nameof(usuario));

            return await _seguranca.GerarTokenAsync(usuario);
        }

        /// <summary>
        /// Valida o token JWT fornecido.
        /// </summary>
        public async Task<bool> ValidarTokenAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
                return false;

            return await _seguranca.ValidarTokenAsync(token);
        }

        /// <summary>
        /// Verifica o código de autenticação de dois fatores (2FA).
        /// </summary>
        public async Task<bool> VerificarDoisFatoresAsync(string codigo)
        {
            if (string.IsNullOrEmpty(codigo))
                return false;

            return await Task.FromResult(_seguranca.VerificarDoisFatores(codigo));
        }
    }
}
