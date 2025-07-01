using Microsoft.Maui.Controls;
using VeiculoCertinho.ViewModels;

namespace VeiculoCertinho.Views
{
    public partial class GaragemPage : ContentPage
    {
        private readonly GaragemViewModel _viewModel;

        public GaragemPage(GaragemViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.CarregarVeiculosAsync();
        }

        private async void OnVeiculoCardTapped(object sender, System.EventArgs e)
        {
            if (_viewModel.VeiculoSelecionado == null)
                return;

            var action = await DisplayActionSheet("O que deseja fazer?", "Cancelar", null, "Editar", "Excluir");

            if (action == "Editar")
            {
                // Navegar para a página de edição, passando o veículo selecionado
                var veiculoId = _viewModel.VeiculoSelecionado.Id;
                await Shell.Current.GoToAsync($"VeiculoPage?veiculoId={veiculoId}");
            }
            else if (action == "Excluir")
            {
                var confirm = await DisplayAlert("Confirmação", "Deseja realmente excluir este veículo?", "Sim", "Não");
                if (confirm)
                {
                    var veiculoId = _viewModel.VeiculoSelecionado.Id;
                    var sucesso = await _viewModel.ExcluirVeiculoAsync(veiculoId);
                    if (sucesso)
                    {
                        await DisplayAlert("Sucesso", "Veículo excluído com sucesso.", "OK");
                    }
                    else
                    {
                        await DisplayAlert("Erro", "Falha ao excluir veículo.", "OK");
                    }
                }
            }
        }

        private async void OnNovoVeiculoClicked(object sender, System.EventArgs e)
        {
            await Shell.Current.GoToAsync("VeiculoPage");
        }
    }
}
