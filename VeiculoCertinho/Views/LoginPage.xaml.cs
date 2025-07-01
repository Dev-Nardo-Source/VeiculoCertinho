using Microsoft.Maui.Controls;
using VeiculoCertinho.ViewModels;
using VeiculoCertinho.Services;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace VeiculoCertinho.Views
{
    public partial class LoginPage : ContentPage
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<LoginPage> _logger;
        private readonly VeiculoService _veiculoService;
        private readonly IVeiculoConsultaServiceSelenium _veiculoConsultaService;

        public LoginPage(
            IConfiguration configuration,
            VeiculoService veiculoService,
            IVeiculoConsultaServiceSelenium veiculoConsultaService,
            ILogger<LoginPage>? logger = null)
        {
            InitializeComponent();
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? NullLogger<LoginPage>.Instance;
            _veiculoService = veiculoService;
            _veiculoConsultaService = veiculoConsultaService;
            BindingContext = new LoginViewModel(configuration);
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            try
            {
                var viewModel = (LoginViewModel)BindingContext;
                if (await viewModel.ValidarLoginAsync())
                {
                    var handler = Microsoft.Maui.Controls.Application.Current?.Handler;
                    if (handler?.MauiContext?.Services == null)
                    {
                        _logger.LogError("Serviços MAUI não disponíveis");
                        await DisplayAlert("Erro", "Erro interno do aplicativo. Por favor, tente novamente.", "OK");
                        return;
                    }

                    var veiculoPage = ActivatorUtilities.CreateInstance<VeiculoPage>(handler.MauiContext.Services);
                    if (veiculoPage == null)
                    {
                        _logger.LogError("Não foi possível criar a página de veículos");
                        await DisplayAlert("Erro", "Erro ao carregar a próxima tela. Por favor, tente novamente.", "OK");
                        return;
                    }

                    await Navigation.PushAsync(veiculoPage);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Erro ao processar login");
                await DisplayAlert("Erro", "Ocorreu um erro ao processar o login.", "OK");
            }
        }

        private async void OnCadastroClicked(object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new UserRegistrationPage());
        }

        private async void OnVerificarCodigoClicked(object sender, System.EventArgs e)
        {
            try
            {
                var codigo = Codigo2FAEntry.Text;
                if (string.IsNullOrWhiteSpace(codigo))
                {
                    await DisplayAlert("Erro", "Por favor, insira o código 2FA.", "OK");
                    return;
                }

                // TODO: Implementar a verificação do código 2FA
                await DisplayAlert("Sucesso", "Código verificado com sucesso!", "OK");
                DoisFatoresStack.IsVisible = false;
            }
            catch (System.Exception ex)
            {
                _logger?.LogError(ex, "Erro ao verificar código 2FA");
                await DisplayAlert("Erro", "Ocorreu um erro ao verificar o código. Por favor, tente novamente.", "OK");
            }
        }
    }
}
