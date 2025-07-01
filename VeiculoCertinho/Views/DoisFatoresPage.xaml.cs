using System;
using VeiculoCertinho.ViewModels;

namespace VeiculoCertinho.Views
{
    public partial class DoisFatoresPage : ContentPage
    {
        private readonly DoisFatoresViewModel _viewModel;

        public DoisFatoresPage()
        {
            InitializeComponent();

            var loggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder => { });
            var baseLogger = loggerFactory.CreateLogger("BaseLogger");
            var doisFatoresLogger = new VeiculoCertinho.Utils.LoggerAdapter<VeiculoCertinho.Services.DoisFatoresService>(baseLogger);

            var doisFatoresService = new VeiculoCertinho.Services.DoisFatoresService(doisFatoresLogger);
            _viewModel = new DoisFatoresViewModel(doisFatoresService);
            BindingContext = _viewModel;
        }

        private async void OnValidarCodigoClicked(object sender, EventArgs e)
        {
            var codigo = CodigoEntry.Text;
            if (string.IsNullOrEmpty(codigo))
            {
                ResultadoLabel.Text = "Por favor, insira o código.";
                return;
            }

            // Para demonstração, geramos um código e validamos
            var codigoGerado = await _viewModel.GerarCodigoAsync("usuarioTeste");
            var valido = await _viewModel.ValidarCodigoAsync("usuarioTeste", codigo);

            ResultadoLabel.Text = valido ? "Código válido!" : "Código inválido.";
        }
    }
}
