using System.ComponentModel.DataAnnotations;

namespace VeiculoCertinho.Models
{
    /// <summary>
    /// Representa um usuário do sistema.
    /// </summary>
    public class Usuario : BaseModel
    {
        private string _nome = string.Empty;
        private string _email = string.Empty;
        private string _senha = string.Empty;
        private string _senhaHash = string.Empty;
        private bool _doisFatoresAtivado;
        private string _perfil = string.Empty;
        private DateTime _ultimoLogin = DateTime.MinValue;

        /// <summary>
        /// Identificador único do usuário.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nome do usuário.
        /// </summary>
        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(100, ErrorMessage = "Nome deve ter no máximo 100 caracteres")]
        public string Nome
        {
            get => _nome;
            set => SetStringProperty(ref _nome, value);
        }

        /// <summary>
        /// Email do usuário.
        /// </summary>
        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email deve ter um formato válido")]
        [StringLength(250, ErrorMessage = "Email deve ter no máximo 250 caracteres")]
        public string Email
        {
            get => _email;
            set => SetStringProperty(ref _email, value?.ToLowerInvariant());
        }

        /// <summary>
        /// Senha do usuário (temporária, não deve ser persistida).
        /// </summary>
        [Required(ErrorMessage = "Senha é obrigatória")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Senha deve ter entre 6 e 100 caracteres")]
        public string Senha
        {
            get => _senha;
            set => SetStringProperty(ref _senha, value);
        }

        /// <summary>
        /// Hash da senha do usuário (para persistência segura).
        /// </summary>
        public string SenhaHash
        {
            get => _senhaHash;
            set => SetStringProperty(ref _senhaHash, value);
        }

        /// <summary>
        /// Indica se a autenticação de dois fatores está ativada.
        /// </summary>
        public bool DoisFatoresAtivado
        {
            get => _doisFatoresAtivado;
            set => SetProperty(ref _doisFatoresAtivado, value);
        }

        /// <summary>
        /// Perfil do usuário (Admin, Usuario, etc.).
        /// </summary>
        [Required(ErrorMessage = "Perfil é obrigatório")]
        [StringLength(50, ErrorMessage = "Perfil deve ter no máximo 50 caracteres")]
        public string Perfil
        {
            get => _perfil;
            set => SetStringProperty(ref _perfil, value);
        }

        /// <summary>
        /// Data do último login do usuário.
        /// </summary>
        public DateTime UltimoLogin
        {
            get => _ultimoLogin;
            set => SetProperty(ref _ultimoLogin, value);
        }

        /// <summary>
        /// Data de criação do usuário (herda de BaseModel.CreatedAt).
        /// </summary>
        public DateTime DataCriacao => CreatedAt;

        /// <summary>
        /// Verifica se o usuário está ativo (logou recentemente).
        /// </summary>
        public bool EstaAtivo => UltimoLogin > DateTime.UtcNow.AddDays(-30);

        /// <summary>
        /// Nome para exibição (nome + email).
        /// </summary>
        public string DisplayName => $"{Nome} ({Email})";

        /// <summary>
        /// Verifica se o usuário é administrador.
        /// </summary>
        public bool IsAdmin => Perfil.Equals("Admin", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Representação string do usuário.
        /// </summary>
        public override string ToString()
        {
            return DisplayName;
        }

        /// <summary>
        /// Atualiza o último login para agora.
        /// </summary>
        public void RegistrarLogin()
        {
            UltimoLogin = DateTime.UtcNow;
        }

        /// <summary>
        /// Limpa dados sensíveis (senha).
        /// </summary>
        public void LimparDadosSensiveis()
        {
            Senha = string.Empty;
        }
    }
}
