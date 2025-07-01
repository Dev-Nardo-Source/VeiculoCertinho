using System;
using System.Text;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Logging;

namespace VeiculoCertinho.Security
{
    public partial class Seguranca
    {
        private readonly string _secretKey;
        private readonly ILogger<Seguranca> _logger;

        public Seguranca(string secretKey, ILogger<Seguranca> logger)
        {
            _secretKey = secretKey ?? throw new ArgumentNullException(nameof(secretKey));
            _logger = logger;
        }

        /// <summary>
        /// Gera um token JWT para o usuário especificado.
        /// </summary>
        /// <param name="usuario">Nome do usuário.</param>
        /// <returns>Token JWT.</returns>
        public string GerarToken(string usuario)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, usuario)
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Valida o token JWT fornecido.
        /// </summary>
        /// <param name="token">Token JWT.</param>
        /// <returns>True se válido, caso contrário false.</returns>
        public bool ValidarToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha na validação do token JWT.");
                return false;
            }
        }
    }
}
