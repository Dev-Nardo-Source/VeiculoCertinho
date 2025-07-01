using System;
using Microsoft.Maui.Controls;
using VeiculoCertinho.Models;
using VeiculoCertinho.ViewModels;
using VeiculoCertinho.Services;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Configuration;
using Mopups.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace VeiculoCertinho.Views
{
    public partial class VeiculoEdicaoPage : ContentPage, INotifyPropertyChanged
    {
        private readonly VeiculoViewModel _viewModel;
        private readonly VeiculoService _veiculoService;

        private int _veiculoId;
        public int VeiculoId
        {
            get => _veiculoId;
            set
            {
                _veiculoId = value;
                OnPropertyChanged();
                _ = CarregarVeiculoAsync(value);
            }
        }

        public VeiculoEdicaoPage()
        {
            InitializeComponent();
            var consultaService = App.Current?.Handler?.MauiContext?.Services.GetService(typeof(IVeiculoConsultaServiceSelenium)) as IVeiculoConsultaServiceSelenium;
            var veiculoService = App.Current?.Handler?.MauiContext?.Services.GetService(typeof(VeiculoService)) as VeiculoService;
            _veiculoService = veiculoService!;
            var logger = App.Current?.Handler?.MauiContext?.Services.GetService(typeof(ILogger<VeiculoViewModel>)) as ILogger<VeiculoViewModel>;
            _viewModel = new VeiculoViewModel(consultaService!, veiculoService!, logger);
            BindingContext = _viewModel;

            CancelarButton.Clicked += OnCancelarClicked;
            SalvarButton.Clicked += OnSalvarClicked;
        }

        private async Task CarregarVeiculoAsync(int veiculoId)
        {
            var veiculo = await _viewModel.ObterVeiculoPorIdAsync(veiculoId);
            if (veiculo != null)
            {
                _viewModel.Veiculo = veiculo;
                // Atualização automática via binding
            }
        }

        public new event PropertyChangedEventHandler? PropertyChanged;
        protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void OnCancelarClicked(object? sender, EventArgs e)
        {
            Navigation.PopAsync();
        }

        private async void OnSalvarClicked(object? sender, EventArgs e)
        {
            if (await ValidarCamposObrigatorios())
            {
                var sucesso = await _veiculoService.AtualizarVeiculoAsync(_viewModel.Veiculo);
                if (sucesso)
                {
                    await DisplayAlert("Sucesso", "Veículo atualizado com sucesso.", "OK");
                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("Erro", "Falha ao atualizar veículo.", "OK");
                }
            }
        }

        private async Task<bool> ValidarCamposObrigatorios()
        {
            if (string.IsNullOrWhiteSpace(_viewModel.Veiculo.Placa))
            {
                await DisplayAlert("Erro", "Por favor, preencha a placa do veículo.", "OK");
                return false;
            }

            if (string.IsNullOrWhiteSpace(_viewModel.Veiculo.Chassi))
            {
                await DisplayAlert("Erro", "Por favor, preencha o chassi do veículo.", "OK");
                return false;
            }

            return true;
        }
    }
}
