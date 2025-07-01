using System;
using VeiculoCertinho.ViewModels;

namespace VeiculoCertinho.Views
{
    public partial class IntegracaoOrgaosPage : ContentPage
    {
        private readonly IntegracaoViewModel _viewModel;

        public IntegracaoOrgaosPage()
        {
            InitializeComponent();

            var loggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder => { });
            var baseLogger = loggerFactory.CreateLogger("BaseLogger");
            var integracaoOrgaosLogger = new VeiculoCertinho.Utils.LoggerAdapter<VeiculoCertinho.Services.IntegracaoOrgaosService>(baseLogger);

            var integracaoOrgaosService = new VeiculoCertinho.Services.IntegracaoOrgaosService(integracaoOrgaosLogger);
            _viewModel = new IntegracaoViewModel(null, integracaoOrgaosService);
            BindingContext = _viewModel;
        }

        private async void OnConsultarMultasClicked(object sender, EventArgs e)
        {
            var placa = PlacaEntry.Text;
            if (string.IsNullOrEmpty(placa))
            {
                ResultadoMultasLabel.Text = "Por favor, insira a placa do veículo.";
                return;
            }

            var resultado = await _viewModel.ConsultarMultasAsync(placa);
            ResultadoMultasLabel.Text = resultado;
        }

        private async void OnDownloadCRLVeClicked(object sender, EventArgs e)
        {
            var placa = PlacaEntry.Text;
            if (string.IsNullOrEmpty(placa))
            {
                ResultadoDownloadLabel.Text = "Por favor, insira a placa do veículo.";
                return;
            }

            var arquivo = await _viewModel.DownloadCRLVeAsync(placa);
            if (arquivo == null || arquivo.Length == 0)
            {
                ResultadoDownloadLabel.Text = "Falha ao baixar o CRLV-e.";
            }
            else
            {
                ResultadoDownloadLabel.Text = "CRLV-e baixado com sucesso (simulado).";
                // Aqui poderia salvar o arquivo localmente ou abrir visualizador PDF
            }
        }
    }
}
