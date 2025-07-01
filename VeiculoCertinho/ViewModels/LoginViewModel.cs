using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Maui.Controls;
using VeiculoCertinho.Utils;
using System.ComponentModel.DataAnnotations;

namespace VeiculoCertinho.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly IConfiguration _configuration;
        private string _email = string.Empty;
        private string _senha = string.Empty;
        private bool _lembrarLogin;

        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email deve ter um formato válido")]
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value?.ToLowerInvariant());
        }

        [Required(ErrorMessage = "Senha é obrigatória")]
        [MinLength(6, ErrorMessage = "Senha deve ter pelo menos 6 caracteres")]
        public string Senha
        {
            get => _senha;
            set => SetProperty(ref _senha, value);
        }

        public bool LembrarLogin
        {
            get => _lembrarLogin;
            set => SetProperty(ref _lembrarLogin, value);
        }

        // Propriedades calculadas
        public bool CanLogin => !IsBusy && !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Senha);
        public string EmailPlaceholder => "Digite seu email";
        public string SenhaPlaceholder => "Digite sua senha";

        // Comandos usando CommandFactory
        public ICommand LoginCommand { get; private set; }
        public ICommand EsqueceuSenhaCommand { get; private set; }
        public ICommand CriarContaCommand { get; private set; }

        public LoginViewModel(IConfiguration configuration, ILogger<LoginViewModel>? logger = null) : base(logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            InitializeCommands();
        }

        private void InitializeCommands()
        {
            LoginCommand = CommandFactory.CreateAsync(async () => await ValidarLoginAsync(), () => CanLogin);

            EsqueceuSenhaCommand = CommandFactory.CreateAsync(ProcessarEsqueceuSenhaAsync, () => !string.IsNullOrWhiteSpace(Email));

            CriarContaCommand = CommandFactory.CreateNavigation("///UserRegistrationPage");
        }

        public async Task<bool> ValidarLoginAsync()
        {
            // Validar campos usando DataAnnotations
            if (!ValidarCampos())
            {
                return false;
            }

            var resultado = await ExecuteWithLoadingAsync(
                async () =>
                {
                    // TODO: Implementar validação com o serviço de autenticação real
                    await Task.Delay(1000); // Simular operação de rede

                    // Validação simples para demonstração
                    if (Email == "admin@teste.com" && Senha == "123456")
                    {
                        return true;
                    }

                    // Simular diferentes tipos de erro
                    if (Email == "admin@teste.com")
                    {
                        throw new UnauthorizedAccessException("Senha incorreta");
                    }

                    throw new Exception("Usuário não encontrado");
                },
                "Validando credenciais..."
            );

            if (resultado == true)
            {
                ShowSuccess("Login realizado com sucesso!");
                
                // Salvar configurações se necessário
                if (LembrarLogin)
                {
                    await SalvarCredenciaisAsync();
                }

                // Navegar para página principal
                await Shell.Current.GoToAsync("//DashboardPage");
                return true;
            }

            return false;
        }

        private async Task ProcessarEsqueceuSenhaAsync()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                ShowError("Digite seu email para recuperar a senha");
                return;
            }

            var resultado = await ExecuteWithLoadingAsync(
                async () =>
                {
                    // TODO: Implementar envio de email de recuperação
                    await Task.Delay(2000); // Simular envio de email
                    return true;
                },
                "Enviando email de recuperação..."
            );

            if (resultado == true)
            {
                ShowSuccess("Email de recuperação enviado! Verifique sua caixa de entrada.");
            }
        }

        private bool ValidarCampos()
        {
            ClearErrors();

            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(Email))
                errors.Add("Email é obrigatório");
            else if (!IsValidEmail(Email))
                errors.Add("Email deve ter um formato válido");

            if (string.IsNullOrWhiteSpace(Senha))
                errors.Add("Senha é obrigatória");
            else if (Senha.Length < 6)
                errors.Add("Senha deve ter pelo menos 6 caracteres");

            if (errors.Any())
            {
                ShowError(string.Join("\n", errors));
                return false;
            }

            return true;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private async Task SalvarCredenciaisAsync()
        {
            try
            {
                // TODO: Implementar salvamento seguro das credenciais
                // Usar Preferences ou Secure Storage
                Preferences.Set("lembrar_login", true);
                Preferences.Set("ultimo_email", Email);
                
                _logger?.LogInformation("Credenciais salvas para lembrar login");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Erro ao salvar credenciais");
            }
        }

        public async Task CarregarCredenciaisSalvasAsync()
        {
            try
            {
                var lembrarLogin = Preferences.Get("lembrar_login", false);
                var ultimoEmail = Preferences.Get("ultimo_email", string.Empty);

                if (lembrarLogin && !string.IsNullOrWhiteSpace(ultimoEmail))
                {
                    Email = ultimoEmail;
                    LembrarLogin = true;
                    ShowSuccess("Dados de login carregados");
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Erro ao carregar credenciais salvas");
            }
        }

        protected override async Task OnLoadAsync()
        {
            await CarregarCredenciaisSalvasAsync();
        }

        public void LimparFormulario()
        {
            Email = string.Empty;
            Senha = string.Empty;
            LembrarLogin = false;
            ClearMessages();
        }
    }
} 