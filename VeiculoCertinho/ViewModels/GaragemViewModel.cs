using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using VeiculoCertinho.Models;
using VeiculoCertinho.Repositories;
using Microsoft.Maui.Controls;
using VeiculoCertinho.Services;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace VeiculoCertinho.ViewModels
{
    public class GaragemViewModel : BaseViewModel
    {
        private readonly VeiculoRepositorio _veiculoRepositorio;
        private readonly IVeiculoConsultaServiceSelenium _consultaService;
        private readonly VeiculoService _veiculoService;
        private readonly ILogger<VeiculoViewModel> _logger;

        public ObservableCollection<VeiculoListItem> Veiculos { get; } = new ObservableCollection<VeiculoListItem>();

        private VeiculoListItem? _veiculoSelecionado;
        public VeiculoListItem? VeiculoSelecionado
        {
            get => _veiculoSelecionado;
            set
            {
                if (_veiculoSelecionado != value)
                {
                    _veiculoSelecionado = value;
                    OnPropertyChanged(nameof(VeiculoSelecionado));
                }
            }
        }

        public ICommand CarregarVeiculosCommand { get; }
        public ICommand IrParaCadastroCommand { get; }
        public ICommand IrParaExclusaoCommand { get; }

        public GaragemViewModel(VeiculoRepositorio veiculoRepositorio, IVeiculoConsultaServiceSelenium consultaService, VeiculoService veiculoService, ILogger<VeiculoViewModel> logger)
        {
            _veiculoRepositorio = veiculoRepositorio;
            _consultaService = consultaService;
            _veiculoService = veiculoService;
            _logger = logger;

            CarregarVeiculosCommand = new Command(async () => await CarregarVeiculosAsync());
            IrParaCadastroCommand = new Command(async () => await IrParaCadastroAsync());
            IrParaExclusaoCommand = new Command(async () => await IrParaExclusaoAsync());
        }

        private async Task IrParaCadastroAsync()
        {
            try
            {
                await Shell.Current.GoToAsync("VeiculoPage");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao navegar para página de cadastro");
                await Shell.Current.DisplayAlert("Erro", "Não foi possível abrir a página de cadastro", "OK");
            }
        }

        private async Task IrParaExclusaoAsync()
        {
            try
            {
                await Shell.Current.GoToAsync("ExcluirVeiculoPage");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao navegar para página de exclusão");
                await Shell.Current.DisplayAlert("Erro", "Não foi possível abrir a página de exclusão", "OK");
            }
        }

        public async Task CarregarVeiculosAsync()
        {
            try
            {
                Veiculos.Clear();
                var lista = await _veiculoRepositorio.ObterTodosAsync();
                foreach (var veiculo in lista)
                {
                    var item = new VeiculoListItem(veiculo);
                    Veiculos.Add(item);
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar veículos");
                await Shell.Current.DisplayAlert("Erro", "Erro ao carregar veículos", "OK");
            }
        }

        public async Task<bool> ExcluirVeiculoAsync(int veiculoId)
        {
            try
            {
                await _veiculoRepositorio.RemoverAsync(veiculoId);
                var veiculoParaRemover = Veiculos.FirstOrDefault(v => v.Id == veiculoId);
                if (veiculoParaRemover != null)
                {
                    Veiculos.Remove(veiculoParaRemover);
                }
                return true;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir veículo");
                await Shell.Current.DisplayAlert("Erro", "Erro ao excluir veículo", "OK");
                return false;
            }
        }
    }

    public class VeiculoListItem : Veiculo
    {
        public string TiposCombustivel
        {
            get
            {
                var tipos = new List<string>();
                if (UsaGasolina) tipos.Add("Gasolina");
                if (UsaEtanol) tipos.Add("Etanol");
                if (UsaGNV) tipos.Add("GNV");
                if (UsaRecargaEletrica) tipos.Add("Elétrico");
                if (UsaDiesel) tipos.Add("Diesel");
                if (UsaHidrogenio) tipos.Add("Hidrogênio");

                return tipos.Any() ? string.Join(", ", tipos) : "Não especificado";
            }
        }

        // Propriedades para binding no XAML
        public string Ano => AnoFabricacao > 0 && AnoModelo > 0 ? $"{AnoFabricacao}/{AnoModelo}" : 
                             AnoFabricacao > 0 ? AnoFabricacao.ToString() : "N/A";
        public string Municipio => MunicipioAtual;
        public string Uf => UfAtual;

        public VeiculoListItem(Veiculo veiculo)
        {
            Id = veiculo.Id;
            Marca = veiculo.Marca;
            Modelo = veiculo.Modelo;
            AnoFabricacao = veiculo.AnoFabricacao;
            AnoModelo = veiculo.AnoModelo; // CORREÇÃO: Adicionar AnoModelo para formar "2018/2019"
            Placa = veiculo.Placa;
            MunicipioAtual = string.IsNullOrWhiteSpace(veiculo.MunicipioAtual) ? "Não informado" : veiculo.MunicipioAtual;
            UfAtual = string.IsNullOrWhiteSpace(veiculo.UfAtual) ? "??" : veiculo.UfAtual;
            UsaGasolina = veiculo.UsaGasolina;
            UsaEtanol = veiculo.UsaEtanol;
            UsaGNV = veiculo.UsaGNV;
            UsaRecargaEletrica = veiculo.UsaRecargaEletrica;
            UsaDiesel = veiculo.UsaDiesel;
            UsaHidrogenio = veiculo.UsaHidrogenio;
        }
    }
}
