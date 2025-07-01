using System;
using VeiculoCertinho.ViewModels;

namespace VeiculoCertinho.Views
{
    public partial class VeiculoEspecificoPage : ContentPage
    {
        private readonly VeiculoEspecificoViewModel _viewModel;

        public VeiculoEspecificoPage()
        {
            InitializeComponent();
            _viewModel = new VeiculoEspecificoViewModel(new Services.VeiculoEspecificoService());
            BindingContext = _viewModel;
        }

        private async void OnMonitorarBateriaClicked(object sender, EventArgs e)
        {
            if (int.TryParse(VeiculoIdEntry.Text, out int veiculoId))
            {
                var resultado = await _viewModel.MonitorarBateriaAsync(veiculoId);
                ResultadoBateriaLabel.Text = resultado;
            }
            else
            {
                ResultadoBateriaLabel.Text = "ID do veículo inválido.";
            }
        }

        private async void OnMonitorarTacografoClicked(object sender, EventArgs e)
        {
            if (int.TryParse(VeiculoIdEntry.Text, out int veiculoId))
            {
                var resultado = await _viewModel.MonitorarTacografoAsync(veiculoId);
                ResultadoTacografoLabel.Text = resultado;
            }
            else
            {
                ResultadoTacografoLabel.Text = "ID do veículo inválido.";
            }
        }

        private async void OnMonitorarCorrenteClicked(object sender, EventArgs e)
        {
            if (int.TryParse(VeiculoIdEntry.Text, out int veiculoId))
            {
                var resultado = await _viewModel.MonitorarCorrenteTransmissaoAsync(veiculoId);
                ResultadoCorrenteLabel.Text = resultado;
            }
            else
            {
                ResultadoCorrenteLabel.Text = "ID do veículo inválido.";
            }
        }
    }
}
