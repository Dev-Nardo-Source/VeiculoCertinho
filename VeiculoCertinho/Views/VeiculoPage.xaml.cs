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
    [QueryProperty(nameof(VeiculoId), "veiculoId")]
    public partial class VeiculoPage : ContentPage, INotifyPropertyChanged
    {
        private readonly VeiculoViewModel _viewModel;
        private bool _isConsultaEmAndamento = false;
        private LoadingPopup? _loadingPopup;

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

        public VeiculoPage(VeiculoViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;

            // Conectar eventos da placa e chassi
            PlacaEntry.TextChanged += PlacaEntry_TextChanged;
            ChassiEntry.TextChanged += ChassiEntry_TextChanged;

            CancelarButton.Clicked += OnCancelarClicked;

            // Inscrever no evento CadastroConcluido
            _viewModel.CadastroConcluido += async (s, e) =>
            {
                var garagemPage = Handler.MauiContext?.Services.GetService<GaragemPage>();
                if (garagemPage != null)
                {
                    await Navigation.PushAsync(garagemPage);
                }
                else
                {
                    await DisplayAlert("Erro", "Não foi possível navegar para a página Garagem.", "OK");
                }
            };

            // Carregar UFs ao inicializar
            Task.Run(async () => await RecarregarUfsAsync());
        }

        // CORREÇÃO 1: Adicionar método RecarregarUfsAsync
        private async Task RecarregarUfsAsync()
        {
            try
            {
                await _viewModel.CarregarUfsAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao carregar UFs: {ex.Message}", "OK");
            }
        }

        private async Task CarregarVeiculoAsync(int veiculoId)
        {
            if (veiculoId <= 0) return;

            var veiculo = await _viewModel.ObterVeiculoPorIdAsync(veiculoId);
            if (veiculo != null)
            {
                _viewModel.Veiculo = veiculo;
            }
        }

        private void PlacaEntry_TextChanged(object? sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.NewTextValue))
            {
                PlacaEntry.Text = e.NewTextValue.ToUpper();
            }
        }

        private void ChassiEntry_TextChanged(object? sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.NewTextValue))
            {
                ChassiEntry.Text = e.NewTextValue.ToUpper();
            }
        }

        private bool ValidarPlaca(string placa)
        {
            if (string.IsNullOrWhiteSpace(placa) || placa.Length < 7)
                return false;

            // Validação Mercosul (AAA9A99)
            if (placa.Length == 7 && char.IsLetter(placa[4]))
            {
                return placa.Take(3).All(char.IsLetter) &&
                       char.IsDigit(placa[3]) &&
                       char.IsLetter(placa[4]) &&
                       placa.Skip(5).All(char.IsDigit);
            }

            // Validação tradicional (AAA9999)
            return placa.Take(3).All(char.IsLetter) && placa.Skip(3).All(char.IsDigit);
        }

        private async void OnConsultarPlacaClicked(object sender, EventArgs e)
        {
            if (_isConsultaEmAndamento)
            {
                await DisplayAlert("Aguarde", "Consulta em andamento. Por favor, aguarde.", "OK");
                return;
            }

            try
            {
                _isConsultaEmAndamento = true;
                _loadingPopup = new LoadingPopup();
                await MopupService.Instance.PushAsync(_loadingPopup);

                PlacaEntry.Text = PlacaEntry.Text.ToUpper().Trim();
                var placa = PlacaEntry.Text;

                if (!ValidarPlaca(placa))
                {
                    await MopupService.Instance.PopAsync();
                    await DisplayAlert("Erro", "Formato de placa incorreto. Deve ser no formato AAA9999 ou AAA9A99.", "OK");
                    return;
                }

                var veiculoExistente = await _viewModel.ObterVeiculoPorPlacaAsync(placa);
                if (veiculoExistente != null)
                {
                    await MopupService.Instance.PopAsync();
                    await DisplayAlert("Alerta", "Veículo já cadastrado.", "OK");
                    return;
                }

                var chassiDigitado = ChassiEntry.Text;
                var encontrado = await _viewModel.ConsultarPlacaAsync(placa, chassiDigitado);
                
                if (encontrado)
                {
                    // Forçar atualização da UI via binding
                    _viewModel.Veiculo = _viewModel.Veiculo;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Ocorreu um erro ao consultar o veículo: {ex.Message}", "OK");
            }
            finally
            {
                _isConsultaEmAndamento = false;
                if (_loadingPopup != null && MopupService.Instance.PopupStack.Count > 0)
                {
                    await MopupService.Instance.PopAsync();
                }
            }
        }

        private void LimparFormulario()
        {
            PlacaEntry.Text = string.Empty;
            ChassiEntry.Text = string.Empty;
            _viewModel.Veiculo = new Veiculo();
            _viewModel.PreenchidoAutomaticamente = false;
            PlacaEntry.Focus();
        }

        private void OnCancelarClicked(object? sender, EventArgs e)
        {
            LimparFormulario();
        }

        private async void OnCadastrarClicked(object sender, EventArgs e)
        {
            if (await ValidarCamposObrigatorios())
            {
                var veiculo = CriarVeiculoDoFormulario();
                _viewModel.Veiculo = veiculo;
                
                // CORREÇÃO 2: Chamar método público CadastrarVeiculoAsync
                await _viewModel.CadastrarVeiculoAsync();
            }
        }

        private async Task<bool> ValidarCamposObrigatorios()
        {
            if (string.IsNullOrWhiteSpace(PlacaEntry.Text))
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

        private Veiculo CriarVeiculoDoFormulario()
        {
            return new Veiculo
            {
                Placa = PlacaEntry.Text?.ToUpper() ?? string.Empty,
                Chassi = ChassiEntry.Text?.ToUpper() ?? string.Empty,
                Marca = _viewModel.Veiculo.Marca,
                Modelo = _viewModel.Veiculo.Modelo,
                AnoFabricacao = _viewModel.Veiculo.AnoFabricacao,
                AnoModelo = _viewModel.Veiculo.AnoModelo,
                Cor = _viewModel.Veiculo.Cor,
                Motor = _viewModel.Veiculo.Motor,
                Cilindrada = _viewModel.Veiculo.Cilindrada,
                Potencia = _viewModel.Veiculo.Potencia,
                Importado = _viewModel.Veiculo.Importado,
                UfOrigem = _viewModel.Veiculo.UfOrigem,
                UfOrigemId = _viewModel.Veiculo.UfOrigemId,
                MunicipioOrigem = _viewModel.Veiculo.MunicipioOrigem,
                UfAtual = _viewModel.Veiculo.UfAtual,
                UfAtualId = _viewModel.Veiculo.UfAtualId,
                MunicipioAtual = _viewModel.Veiculo.MunicipioAtual,
                Segmento = _viewModel.Veiculo.Segmento,
                EspecieVeiculo = _viewModel.Veiculo.EspecieVeiculo,
                Passageiros = _viewModel.Veiculo.Passageiros,
                Observacoes = _viewModel.Veiculo.Observacoes,
                UsaGasolina = _viewModel.Veiculo.UsaGasolina,
                UsaEtanol = _viewModel.Veiculo.UsaEtanol,
                UsaGNV = _viewModel.Veiculo.UsaGNV,
                UsaRecargaEletrica = _viewModel.Veiculo.UsaRecargaEletrica,
                UsaDiesel = _viewModel.Veiculo.UsaDiesel,
                UsaHidrogenio = _viewModel.Veiculo.UsaHidrogenio
            };
        }
    }
}
