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

        // Remover declarações manuais dos controles para evitar conflito com campos gerados pelo XAML
        // private Entry? UfOrigemEntry;
        // private Entry? MunicipioOrigemEntry;
        // private Entry? UfAtualEntry;
        // private Entry? MunicipioAtualEntry;

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

            // Conectar eventos da placa e chassi com checagem de null para evitar warning
            PlacaEntry.TextChanged += PlacaEntry_TextChanged;
            ChassiEntry.TextChanged += ChassiEntry_TextChanged;

            CancelarButton.Clicked += OnCancelarClicked;

            // Inscrever no evento CadastroConcluido para navegação após cadastro
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
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            // Recarregar UFs quando a página aparecer
            await _viewModel.RecarregarUfsAsync();
        }

        private async Task CarregarVeiculoAsync(int veiculoId)
        {
            var veiculo = await _viewModel.ObterVeiculoPorIdAsync(veiculoId);
            if (veiculo != null)
            {
                _viewModel.Veiculo = veiculo;

                // Atualizar campos do formulário
                if (PlacaEntry != null)
                    PlacaEntry.Text = veiculo.Placa;
                if (ChassiEntry != null)
                    ChassiEntry.Text = veiculo.Chassi;
                // Atualização automática via binding
            }
        }

        public new event PropertyChangedEventHandler? PropertyChanged;
        protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private async void ChassiEntry_TextChanged(object? sender, TextChangedEventArgs e)
        {
            if (ChassiEntry.Text?.Length == 17 && PlacaEntry.Text?.Length == 7)
            {
                await TentarConsultarPlaca();
            }
        }

        private async void PlacaEntry_TextChanged(object? sender, TextChangedEventArgs e)
        {
            if (PlacaEntry.Text?.Length == 7 && ChassiEntry.Text?.Length == 17)
            {
                var placa = PlacaEntry.Text.ToUpper().Trim();

                // Verificar se veículo já está cadastrado ao completar a placa
                var veiculoExistente = await _viewModel.ObterVeiculoPorPlacaAsync(placa);
                if (veiculoExistente != null)
                {
                    await DisplayAlert("Alerta", "Veículo já cadastrado.", "OK");
                    LimparFormulario();
                    return;
                }

                await TentarConsultarPlaca();
            }
        }

        private bool ValidarPlacaMercosul(string placa)
        {
            if (string.IsNullOrWhiteSpace(placa) || placa.Length != 7)
                return false;

            return char.IsLetter(placa[0]) &&
                   char.IsLetter(placa[1]) &&
                   char.IsLetter(placa[2]) &&
                   char.IsDigit(placa[3]) &&
                   char.IsLetter(placa[4]) &&
                   char.IsDigit(placa[5]) &&
                   char.IsDigit(placa[6]);
        }

        private bool ValidarPlacaTradicional(string placa)
        {
            if (string.IsNullOrWhiteSpace(placa) || placa.Length != 7)
                return false;

            return char.IsLetter(placa[0]) &&
                   char.IsLetter(placa[1]) &&
                   char.IsLetter(placa[2]) &&
                   char.IsDigit(placa[3]) &&
                   char.IsDigit(placa[4]) &&
                   char.IsDigit(placa[5]) &&
                   char.IsDigit(placa[6]);
        }

        private bool ValidarPlaca(string placa)
        {
            if (string.IsNullOrWhiteSpace(placa))
                return false;

            placa = placa.Trim().ToUpper();
            return ValidarPlacaMercosul(placa) || ValidarPlacaTradicional(placa);
        }

        private async Task TentarConsultarPlaca()
        {
            if (_isConsultaEmAndamento)
                return;

            // Verificar se placa e chassi estão preenchidos corretamente antes de consultar
            if (string.IsNullOrWhiteSpace(PlacaEntry.Text) || PlacaEntry.Text.Length != 7)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(ChassiEntry.Text) || ChassiEntry.Text.Length != 17)
            {
                return;
            }

            try
            {
                _isConsultaEmAndamento = true;
                _loadingPopup = new LoadingPopup("Aguarde...!!!");
                await MopupService.Instance.PushAsync(_loadingPopup);

                // Forçar texto em maiúsculo
                PlacaEntry.Text = PlacaEntry.Text.ToUpper().Trim();
                var placa = PlacaEntry.Text;

                if (!ValidarPlaca(placa))
                {
                    await MopupService.Instance.PopAsync();
                    await DisplayAlert("Erro", "Formato de placa incorreto. Deve ser no formato AAA9999 ou AAA9A99.", "OK");
                    return;
                }

                // Verificar se veículo já está cadastrado
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
                    // Atualizar o ViewModel para refletir no binding e atualizar a UI automaticamente
                    _viewModel.Veiculo = _viewModel.Veiculo; // Forçar notificação de propriedade se necessário

                    // Remover referências diretas aos controles para evitar erros de compilação
                    // A atualização da UI será feita via binding no XAML
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
                await _viewModel.CadastrarVeiculoAsync();
                
                // Exibir mensagem de sucesso
                // await DisplayAlert("Sucesso", "Veículo cadastrado com sucesso!", "OK");
                
                // Navegar para a tela Garagem
                if (Handler != null && Handler.MauiContext != null)
                {
                    var garagemPage = Handler.MauiContext.Services.GetService<GaragemPage>();
                    if (garagemPage != null)
                    {
                        await Navigation.PushAsync(garagemPage);
                    }
                    else
                    {
                        await DisplayAlert("Erro", "Não foi possível navegar para a página Garagem.", "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Erro", "Contexto MauiContext ou Handler está nulo.", "OK");
                }
            }
            else
            {
                await DisplayAlert("Erro", "Contexto MauiContext ou Services está nulo.", "OK");
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
