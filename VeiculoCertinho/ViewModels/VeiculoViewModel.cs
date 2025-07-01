using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using VeiculoCertinho.Models;
using VeiculoCertinho.Services;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using VeiculoCertinho.Views;
using Mopups.Services;
using System.Linq;
using VeiculoCertinho.Repositories;
using VeiculoCertinho.Utils;

namespace VeiculoCertinho.ViewModels
{
    public class VeiculoViewModel : BaseViewModel
    {
        private readonly IVeiculoConsultaServiceSelenium _consultaService;
        private readonly VeiculoService _veiculoService;
        private readonly UfRepositorio? _ufRepositorio;
        private readonly MunicipioService? _municipioService;
        
        private Veiculo _veiculo = new Veiculo();
        private bool _preenchidoAutomaticamente;
        private CancellationTokenSource? _cancellationTokenSource;
        private ObservableCollection<Uf> _ufs = new ObservableCollection<Uf>();
        private ObservableCollection<Municipio> _municipiosOrigem = new ObservableCollection<Municipio>();
        private ObservableCollection<Municipio> _municipiosAtual = new ObservableCollection<Municipio>();
        private Uf? _ufOrigemSelecionada;
        private Uf? _ufAtualSelecionada;
        private Municipio? _municipioOrigemSelecionado;
        private Municipio? _municipioAtualSelecionado;
        private Veiculo? _veiculoSelecionado;
        private VeiculoDetalhes? _detalhesSelecionados;

        // Eventos
        public event EventHandler? CadastroConcluido;

        // Collections
        public ObservableCollection<Veiculo> Veiculos { get; set; } = new ObservableCollection<Veiculo>();
        public ObservableCollection<VeiculoDetalhes> VeiculosDetalhes { get; set; } = new ObservableCollection<VeiculoDetalhes>();

        // Propriedades principais
        public Veiculo Veiculo
        {
            get => _veiculo;
            set
            {
                if (value == null) return;
                
                if (SetProperty(ref _veiculo, value))
                {
                    // Notificar mudanças nas propriedades relacionadas
                    OnPropertyChanged(nameof(CanRegister));
                    NotificarPropriedadesVeiculo();
                    NotificarPropriedadesCombustivel();
                }
            }
        }

        public Veiculo? VeiculoSelecionado
        {
            get => _veiculoSelecionado;
            set => SetProperty(ref _veiculoSelecionado, value);
        }

        public VeiculoDetalhes? DetalhesSelecionados
        {
            get => _detalhesSelecionados;
            set => SetProperty(ref _detalhesSelecionados, value);
        }

        public bool PreenchidoAutomaticamente
        {
            get => _preenchidoAutomaticamente;
            set
            {
                if (SetProperty(ref _preenchidoAutomaticamente, value))
                {
                    OnPropertyChanged(nameof(MunicipioAtualHabilitado));
                }
            }
        }

        public bool MunicipioAtualHabilitado => !IsBusy && !PreenchidoAutomaticamente && UfAtualSelecionada != null;
        public bool CanRegister => !IsBusy && Veiculo != null;

        // Propriedades de UF e Município
        public ObservableCollection<Uf> Ufs
        {
            get => _ufs;
            set => SetProperty(ref _ufs, value);
        }

        public ObservableCollection<Municipio> MunicipiosOrigem
        {
            get => _municipiosOrigem;
            set => SetProperty(ref _municipiosOrigem, value);
        }

        public ObservableCollection<Municipio> MunicipiosAtual
        {
            get => _municipiosAtual;
            set => SetProperty(ref _municipiosAtual, value);
        }

        public Uf? UfOrigemSelecionada
        {
            get => _ufOrigemSelecionada;
            set
            {
                if (SetProperty(ref _ufOrigemSelecionada, value) && value != null)
                {
                    Veiculo.SincronizarUfOrigem(value.Id, value.Sigla);
                    _ = CarregarMunicipiosOrigemAsync(value.Id);
                }
            }
        }

        public Uf? UfAtualSelecionada
        {
            get => _ufAtualSelecionada;
            set
            {
                if (SetProperty(ref _ufAtualSelecionada, value))
                {
                    OnPropertyChanged(nameof(MunicipioAtualHabilitado));
                    if (value != null)
                    {
                        Veiculo.SincronizarUfAtual(value.Id, value.Sigla);
                        _ = CarregarMunicipiosAtualAsync(value.Id);
                    }
                }
            }
        }

        public Municipio? MunicipioOrigemSelecionado
        {
            get => _municipioOrigemSelecionado;
            set
            {
                if (SetProperty(ref _municipioOrigemSelecionado, value) && value != null)
                {
                    Veiculo.MunicipioOrigem = value.Nome;
                }
            }
        }

        public Municipio? MunicipioAtualSelecionado
        {
            get => _municipioAtualSelecionado;
            set
            {
                if (SetProperty(ref _municipioAtualSelecionado, value) && value != null)
                {
                    Veiculo.MunicipioAtual = value.Nome;
                }
            }
        }

        // Propriedades de Combustível (delegadas para o modelo)
        public bool UsaGasolina
        {
            get => Veiculo.UsaGasolina;
            set { Veiculo.UsaGasolina = value; OnPropertyChanged(); }
        }

        public bool UsaEtanol
        {
            get => Veiculo.UsaEtanol;
            set { Veiculo.UsaEtanol = value; OnPropertyChanged(); }
        }

        public bool UsaGNV
        {
            get => Veiculo.UsaGNV;
            set { Veiculo.UsaGNV = value; OnPropertyChanged(); }
        }

        public bool UsaRecargaEletrica
        {
            get => Veiculo.UsaRecargaEletrica;
            set { Veiculo.UsaRecargaEletrica = value; OnPropertyChanged(); }
        }

        public bool UsaDiesel
        {
            get => Veiculo.UsaDiesel;
            set { Veiculo.UsaDiesel = value; OnPropertyChanged(); }
        }

        public bool UsaHidrogenio
        {
            get => Veiculo.UsaHidrogenio;
            set { Veiculo.UsaHidrogenio = value; OnPropertyChanged(); }
        }

        // Comandos usando CommandFactory
        public ICommand ConsultarPlacaCommand { get; private set; }
        public ICommand CadastrarVeiculoCommand { get; private set; }

        public VeiculoViewModel(IVeiculoConsultaServiceSelenium consultaService, VeiculoService veiculoService, 
            ILogger<VeiculoViewModel>? logger = null, UfRepositorio? ufRepositorio = null, 
            MunicipioService? municipioService = null) : base(logger)
        {
            _consultaService = consultaService;
            _veiculoService = veiculoService;
            _ufRepositorio = ufRepositorio;
            _municipioService = municipioService;
            
            InitializeCommands();
        }

        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            // Usando CommandFactory para comandos otimizados
            SaveCommand = CommandFactory.CreateAsyncWithLoading(
                execute: SalvarVeiculoAsync,
                setBusy: busy => IsBusy = busy,
                canExecute: () => CanRegister,
                onError: ex => ShowError($"Erro ao salvar veículo: {ex.Message}"),
                logger: _logger
            );

            CancelCommand = CommandFactory.Create(
                execute: LimparFormulario,
                canExecute: () => !IsBusy
            );

            ConsultarPlacaCommand = CommandFactory.CreateAsync(
                execute: () => ConsultarPlacaAsync(Veiculo.Placa),
                canExecute: () => !IsBusy && ValidarPlaca(Veiculo.Placa),
                onError: ex => ShowError($"Erro ao consultar placa: {ex.Message}"),
                logger: _logger
            );

            CadastrarVeiculoCommand = CommandFactory.CreateAsyncWithLoading(
                execute: CadastrarVeiculoAsync,
                setBusy: busy => IsBusy = busy,
                canExecute: () => CanRegister,
                onError: ex => ShowError($"Erro ao cadastrar veículo: {ex.Message}"),
                logger: _logger
            );
        }

        protected override async Task OnLoadAsync()
        {
            await CarregarUfsAsync();
        }

        // Métodos de negócio simplificados
        public async Task<Veiculo?> ObterVeiculoPorPlacaAsync(string placa)
        {
            return await ExecuteWithLoadingAsync(
                () => _veiculoService.ObterVeiculoPorPlacaAsync(placa),
                "Buscando veículo por placa..."
            );
        }

        public async Task<Veiculo?> ObterVeiculoPorIdAsync(int id)
        {
            return await ExecuteWithLoadingAsync(
                () => _veiculoService.ObterVeiculoPorIdAsync(id),
                "Buscando veículo por ID..."
            );
        }

        public bool ValidarPlaca(string placa)
        {
            if (string.IsNullOrWhiteSpace(placa) || placa.Length < 7) 
                return false;

            // Validação Mercosul (formato: ABC1D23)
            if (placa.Length == 7 && char.IsLetter(placa[4]))
                return ValidarPlacaMercosul(placa);

            // Validação tradicional (formato: ABC-1234)
            return ValidarPlacaTradicional(placa);
        }

        public async Task<bool> ConsultarPlacaAsync(string placa, string? chassiDigitado = null)
        {
            return await ExecuteWithLoadingAsync(async () =>
            {
                if (!ValidarPlaca(placa))
                {
                    ShowError("Placa inválida. Verifique o formato.");
                    return false;
                }

                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource = new CancellationTokenSource();

                var veiculoConsultado = await _consultaService.ConsultarVeiculoPorPlacaAsync(placa, _cancellationTokenSource.Token);

                if (veiculoConsultado == null)
                {
                    ShowError("Veículo não encontrado para a placa informada.");
                    return false;
                }

                if (!string.IsNullOrEmpty(chassiDigitado) && !ValidarChassiConsultado(chassiDigitado, veiculoConsultado.Chassi ?? ""))
                {
                    ShowError("Chassi informado não confere com o chassi do veículo consultado.");
                    return false;
                }

                AtualizarVeiculoExcetoChassi(veiculoConsultado);
                PreenchidoAutomaticamente = true;
                ShowSuccess("Dados do veículo carregados com sucesso!");

                return true;
            }, "Consultando placa...") ?? false;
        }

        private async Task SalvarVeiculoAsync()
        {
            if (Veiculo == null)
            {
                ShowError("Nenhum veículo para salvar.");
                return;
            }

            if (!Veiculo.IsValid)
            {
                var errors = Veiculo.Validate();
                ShowError($"Veículo inválido: {string.Join(", ", errors.Select(e => e.ErrorMessage))}");
                return;
            }

            var sucesso = await _veiculoService.CadastrarVeiculoAsync(Veiculo);
            
            if (sucesso)
            {
                ShowSuccess("Veículo salvo com sucesso!");
                CadastroConcluido?.Invoke(this, EventArgs.Empty);
                LimparFormulario();
            }
            else
            {
                ShowError("Falha ao salvar o veículo.");
            }
        }

        private async Task CadastrarVeiculoAsync()
        {
            await SalvarVeiculoAsync();
        }

        private void LimparFormulario()
        {
            Veiculo = new Veiculo();
            PreenchidoAutomaticamente = false;
            UfOrigemSelecionada = null;
            UfAtualSelecionada = null;
            MunicipioOrigemSelecionado = null;
            MunicipioAtualSelecionado = null;
            ClearMessages();
        }

        // Métodos auxiliares privados
        private bool ValidarPlacaMercosul(string placa)
        {
            return placa.Take(3).All(char.IsLetter) &&
                   char.IsDigit(placa[3]) &&
                   char.IsLetter(placa[4]) &&
                   placa.Skip(5).All(char.IsDigit);
        }

        private bool ValidarPlacaTradicional(string placa)
        {
            var placaLimpa = placa.Replace("-", "");
            return placaLimpa.Length == 7 &&
                   placaLimpa.Take(3).All(char.IsLetter) &&
                   placaLimpa.Skip(3).All(char.IsDigit);
        }

        private bool ValidarChassiConsultado(string chassiDigitado, string chassiConsultado)
        {
            if (string.IsNullOrWhiteSpace(chassiDigitado) || string.IsNullOrWhiteSpace(chassiConsultado))
                return false;

            var ultimos7Digitado = chassiDigitado.Length >= 7 ? chassiDigitado[^7..] : chassiDigitado;
            var chassiConsultadoLimpo = chassiConsultado.Replace("*", "");
            var ultimos7Consultado = chassiConsultadoLimpo.Length >= 7 ? chassiConsultadoLimpo[^7..] : chassiConsultadoLimpo;

            return ultimos7Digitado.Equals(ultimos7Consultado, StringComparison.OrdinalIgnoreCase);
        }

        private void AtualizarVeiculoExcetoChassi(Veiculo veiculoConsultado)
        {
            Veiculo.Placa = veiculoConsultado.Placa;
            Veiculo.Marca = veiculoConsultado.Marca;
            Veiculo.Modelo = veiculoConsultado.Modelo;
            Veiculo.AnoFabricacao = veiculoConsultado.AnoFabricacao;
            Veiculo.AnoModelo = veiculoConsultado.AnoModelo;
            Veiculo.Cor = veiculoConsultado.Cor;
            Veiculo.Motor = veiculoConsultado.Motor;
            Veiculo.Cilindrada = veiculoConsultado.Cilindrada;
            Veiculo.Potencia = veiculoConsultado.Potencia;
            Veiculo.Importado = veiculoConsultado.Importado;
            Veiculo.UfOrigem = veiculoConsultado.UfOrigem;
            Veiculo.UfOrigemId = veiculoConsultado.UfOrigemId;
            Veiculo.MunicipioOrigem = veiculoConsultado.MunicipioOrigem;
            Veiculo.UfAtual = veiculoConsultado.UfOrigem;
            Veiculo.UfAtualId = veiculoConsultado.UfOrigemId;
            Veiculo.MunicipioAtual = veiculoConsultado.MunicipioOrigem;
            Veiculo.Segmento = veiculoConsultado.Segmento;
            Veiculo.EspecieVeiculo = veiculoConsultado.EspecieVeiculo;
            Veiculo.Passageiros = veiculoConsultado.Passageiros;
            Veiculo.Observacoes = veiculoConsultado.Observacoes;

            // Atualizar combustíveis
            Veiculo.UsaGasolina = veiculoConsultado.UsaGasolina;
            Veiculo.UsaEtanol = veiculoConsultado.UsaEtanol;
            Veiculo.UsaGNV = veiculoConsultado.UsaGNV;
            Veiculo.UsaRecargaEletrica = veiculoConsultado.UsaRecargaEletrica;
            Veiculo.UsaDiesel = veiculoConsultado.UsaDiesel;
            Veiculo.UsaHidrogenio = veiculoConsultado.UsaHidrogenio;

            // Atualizar pickers
            _ = AtualizarPickersComDadosConsulta(veiculoConsultado);
        }

        private async Task AtualizarPickersComDadosConsulta(Veiculo veiculoConsultado)
        {
            if (string.IsNullOrWhiteSpace(veiculoConsultado.UfOrigem)) return;

            var ufOrigem = Ufs.FirstOrDefault(u => u.Sigla.Equals(veiculoConsultado.UfOrigem, StringComparison.OrdinalIgnoreCase));
            if (ufOrigem == null) return;

            await CarregarMunicipiosOrigemAsync(ufOrigem.Id);
            await CarregarMunicipiosAtualAsync(ufOrigem.Id);

            UfOrigemSelecionada = ufOrigem;
            UfAtualSelecionada = ufOrigem;

            if (!string.IsNullOrWhiteSpace(veiculoConsultado.MunicipioOrigem))
            {
                MunicipioOrigemSelecionado = MunicipiosOrigem.FirstOrDefault(m => 
                    m.Nome.Equals(veiculoConsultado.MunicipioOrigem, StringComparison.OrdinalIgnoreCase));
                MunicipioAtualSelecionado = MunicipiosAtual.FirstOrDefault(m => 
                    m.Nome.Equals(veiculoConsultado.MunicipioOrigem, StringComparison.OrdinalIgnoreCase));
            }
        }

        private async Task CarregarUfsAsync()
        {
            var ufs = await ExecuteWithLoadingAsync(async () =>
            {
                return _ufRepositorio != null ? await _ufRepositorio.ObterTodosAsync() : new List<Uf>();
            }, "Carregando UFs...");

            if (ufs?.Any() == true)
            {
                Ufs.Clear();
                foreach (var uf in ufs.OrderBy(u => u.Nome))
                {
                    Ufs.Add(uf);
                }
            }
        }

        private async Task CarregarMunicipiosOrigemAsync(int ufId)
        {
            var municipios = await ExecuteWithLoadingAsync(async () =>
            {
                return _municipioService != null ? await _municipioService.ObterMunicipiosPorUfAsync(ufId) : new List<Municipio>();
            });

            if (municipios?.Any() == true)
            {
                MunicipiosOrigem.Clear();
                foreach (var municipio in municipios.OrderBy(m => m.Nome))
                {
                    MunicipiosOrigem.Add(municipio);
                }
            }
        }

        private async Task CarregarMunicipiosAtualAsync(int ufId)
        {
            var municipios = await ExecuteWithLoadingAsync(async () =>
            {
                return _municipioService != null ? await _municipioService.ObterMunicipiosPorUfAsync(ufId) : new List<Municipio>();
            });

            if (municipios?.Any() == true)
            {
                MunicipiosAtual.Clear();
                foreach (var municipio in municipios.OrderBy(m => m.Nome))
                {
                    MunicipiosAtual.Add(municipio);
                }
            }
        }

        private void NotificarPropriedadesVeiculo()
        {
            OnPropertyChanged($"{nameof(Veiculo)}.{nameof(Veiculo.UfOrigem)}");
            OnPropertyChanged($"{nameof(Veiculo)}.{nameof(Veiculo.MunicipioOrigem)}");
            OnPropertyChanged($"{nameof(Veiculo)}.{nameof(Veiculo.Marca)}");
            OnPropertyChanged($"{nameof(Veiculo)}.{nameof(Veiculo.Modelo)}");
            OnPropertyChanged($"{nameof(Veiculo)}.{nameof(Veiculo.AnoFabricacao)}");
            OnPropertyChanged($"{nameof(Veiculo)}.{nameof(Veiculo.AnoModelo)}");
            OnPropertyChanged($"{nameof(Veiculo)}.{nameof(Veiculo.Cor)}");
            OnPropertyChanged($"{nameof(Veiculo)}.{nameof(Veiculo.Placa)}");
        }

        private void NotificarPropriedadesCombustivel()
        {
            OnPropertyChanged(nameof(UsaGasolina));
            OnPropertyChanged(nameof(UsaEtanol));
            OnPropertyChanged(nameof(UsaGNV));
            OnPropertyChanged(nameof(UsaRecargaEletrica));
            OnPropertyChanged(nameof(UsaDiesel));
            OnPropertyChanged(nameof(UsaHidrogenio));
        }
    }
}
