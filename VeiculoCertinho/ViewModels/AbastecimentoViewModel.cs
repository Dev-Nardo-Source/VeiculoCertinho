using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using VeiculoCertinho.Models;
using VeiculoCertinho.Services;
using Microsoft.Maui.Controls;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Linq;
using VeiculoCertinho.Utils;
using System.ComponentModel.DataAnnotations;

namespace VeiculoCertinho.ViewModels
{
    public class AbastecimentoViewModel : BaseViewModel
    {
        private readonly AbastecimentoService? _service;
        private readonly FirebaseHelper _firebaseHelper;

        // Propriedades de entrada
        private int _veiculoId;
        private string _tipoCombustivel = string.Empty;
        private decimal _precoPorUnidade;
        private decimal _quantidade;
        private decimal _valorTotal;
        private int _quilometragemAtual;
        private string _posto = string.Empty;
        private DateTime _dataAbastecimento = DateTime.Now;
        private string _observacoes = string.Empty;
        
        // Estado da UI
        private string _consumoMensagem = "Consumo: -";
        private Abastecimento? _abastecimentoSelecionado;
        private string _filtroTipoCombustivel = string.Empty;
        private bool _isCalculandoConsumo = false;

        // Coleções
        public ObservableCollection<Abastecimento> Abastecimentos { get; private set; } = new ObservableCollection<Abastecimento>();

        // Lista de tipos de combustível disponíveis
        public List<string> TiposCombustivelDisponiveis { get; } = new List<string>
        {
            "Gasolina", "Etanol", "Diesel", "GNV", "Elétrico", "AdBlue", "Hidrogênio"
        };

        [Required(ErrorMessage = "Veículo é obrigatório")]
        [Range(1, int.MaxValue, ErrorMessage = "Veículo deve ser selecionado")]
        public int VeiculoId
        {
            get => _veiculoId;
            set => SetProperty(ref _veiculoId, value);
        }

        [Required(ErrorMessage = "Tipo de combustível é obrigatório")]
        public string TipoCombustivel
        {
            get => _tipoCombustivel;
            set
            {
                if (SetProperty(ref _tipoCombustivel, value))
                {
                    AtualizarConsumoAsync();
                }
            }
        }

        [Required(ErrorMessage = "Preço por unidade é obrigatório")]
        [Range(0.01, 999.99, ErrorMessage = "Preço deve estar entre R$ 0,01 e R$ 999,99")]
        public decimal PrecoPorUnidade
        {
            get => _precoPorUnidade;
            set
            {
                if (SetProperty(ref _precoPorUnidade, value))
                {
                    CalcularCampoFaltante(nameof(PrecoPorUnidade));
                }
            }
        }

        [Required(ErrorMessage = "Quantidade é obrigatória")]
        [Range(0.1, 9999.9, ErrorMessage = "Quantidade deve estar entre 0,1 e 9999,9")]
        public decimal Quantidade
        {
            get => _quantidade;
            set
            {
                if (SetProperty(ref _quantidade, value))
                {
                    CalcularCampoFaltante(nameof(Quantidade));
                }
            }
        }

        public decimal ValorTotal
        {
            get => _valorTotal;
            set
            {
                if (SetProperty(ref _valorTotal, value))
                {
                    CalcularCampoFaltante(nameof(ValorTotal));
                }
            }
        }

        [Required(ErrorMessage = "Quilometragem é obrigatória")]
        [Range(0, int.MaxValue, ErrorMessage = "Quilometragem deve ser positiva")]
        public int QuilometragemAtual
        {
            get => _quilometragemAtual;
            set => SetProperty(ref _quilometragemAtual, value);
        }

        [Required(ErrorMessage = "Posto é obrigatório")]
        public string Posto
        {
            get => _posto;
            set => SetProperty(ref _posto, value);
        }

        public DateTime DataAbastecimento
        {
            get => _dataAbastecimento;
            set => SetProperty(ref _dataAbastecimento, value);
        }

        public string Observacoes
        {
            get => _observacoes;
            set => SetProperty(ref _observacoes, value);
        }

        public Abastecimento? AbastecimentoSelecionado
        {
            get => _abastecimentoSelecionado;
            set
            {
                if (SetProperty(ref _abastecimentoSelecionado, value))
                {
                    CarregarAbastecimentoParaEdicao();
                }
            }
        }

        public string ConsumoMensagem
        {
            get => _consumoMensagem;
            private set => SetProperty(ref _consumoMensagem, value);
        }

        public string FiltroTipoCombustivel
        {
            get => _filtroTipoCombustivel;
            set
            {
                if (SetProperty(ref _filtroTipoCombustivel, value))
                {
                    FiltrarAbastecimentosAsync();
                }
            }
        }

        // Propriedades calculadas
        public bool CanSalvar => !IsBusy && VeiculoId > 0 && !string.IsNullOrWhiteSpace(TipoCombustivel) && 
                                 PrecoPorUnidade > 0 && Quantidade > 0 && QuilometragemAtual >= 0 && !string.IsNullOrWhiteSpace(Posto);
        public bool CanAtualizar => !IsBusy && AbastecimentoSelecionado != null && CanSalvar;
        public bool CanRemover => !IsBusy && AbastecimentoSelecionado != null;
        public bool IsEdicao => AbastecimentoSelecionado != null;
        public string TituloFormulario => IsEdicao ? "Editar Abastecimento" : "Registrar Abastecimento";
        public string TextoBotaoSalvar => IsEdicao ? "Atualizar" : "Registrar";

        // Comandos usando CommandFactory
        public ICommand SalvarCommand { get; private set; }
        public ICommand RemoverCommand { get; private set; }
        public ICommand LimparCommand { get; private set; }
        public ICommand CalcularConsumoCommand { get; private set; }
        public ICommand SelecionarAbastecimentoCommand { get; private set; }

        // Construtor padrão para XAML
        public AbastecimentoViewModel() : base(null)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            _firebaseHelper = new FirebaseHelper(configuration);
            _service = null;
            InitializeCommands();
        }

        public AbastecimentoViewModel(AbastecimentoService service, IConfiguration configuration, ILogger<AbastecimentoViewModel>? logger = null) : base(logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _firebaseHelper = new FirebaseHelper(configuration);
            InitializeCommands();
        }

        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            SalvarCommand = CommandFactory.CreateAsync(SalvarAbastecimentoAsync, () => CanSalvar);

            RemoverCommand = CommandFactory.CreateWithConfirmation(
                RemoverAbastecimentoAsync,
                "Tem certeza que deseja remover este abastecimento?",
                "Remover Abastecimento",
                () => CanRemover
            );

            LimparCommand = CommandFactory.CreateAsync(LimparFormularioAsync);

            CalcularConsumoCommand = CommandFactory.CreateAsync(CalcularConsumoAsync, () => Abastecimentos.Count >= 2);

            SelecionarAbastecimentoCommand = CommandFactory.CreateAsync<Abastecimento>(
                abastecimento => SelecionarAbastecimentoAsync(abastecimento));
        }

        protected override async Task OnLoadAsync()
        {
            await CarregarAbastecimentosAsync();
        }

        public async Task CarregarAbastecimentosAsync()
        {
            if (_service == null)
            {
                ShowError("Serviço não disponível");
                return;
            }

            var abastecimentos = await ExecuteWithLoadingAsync(
                async () => await _service.ObterTodosAsync(),
                "Carregando abastecimentos..."
            );

            if (abastecimentos != null)
            {
                Abastecimentos.Clear();
                foreach (var abastecimento in abastecimentos.OrderByDescending(a => a.Data))
                {
                    Abastecimentos.Add(abastecimento);
                }
                ShowSuccess($"{abastecimentos.Count} abastecimentos carregados");
                await AtualizarConsumoAsync();
            }
        }

        private async Task SalvarAbastecimentoAsync()
        {
            if (!ValidarDados())
            {
                return;
            }

            var sucesso = await ExecuteWithLoadingAsync(
                async () =>
                {
                    if (IsEdicao)
                    {
                        // Atualizar abastecimento existente
                        AbastecimentoSelecionado!.VeiculoId = VeiculoId;
                        AbastecimentoSelecionado.Data = DataAbastecimento;
                        AbastecimentoSelecionado.TipoCombustivel = TipoCombustivel;
                        AbastecimentoSelecionado.PrecoPorUnidade = PrecoPorUnidade;
                        AbastecimentoSelecionado.Quantidade = Quantidade;
                        AbastecimentoSelecionado.QuilometragemAtual = QuilometragemAtual;
                        AbastecimentoSelecionado.Posto = Posto;
                        // ValorTotal é calculado automaticamente no modelo

                        await _service.AtualizarAsync(AbastecimentoSelecionado);
                        return "Abastecimento atualizado com sucesso!";
                    }
                    else
                    {
                        // Criar novo abastecimento
                        var novoAbastecimento = new Abastecimento
                        {
                            VeiculoId = VeiculoId,
                            Data = DataAbastecimento,
                            TipoCombustivel = TipoCombustivel,
                            PrecoPorUnidade = PrecoPorUnidade,
                            Quantidade = Quantidade,
                            QuilometragemAtual = QuilometragemAtual,
                            Posto = Posto
                            // ValorTotal é calculado automaticamente no modelo
                        };

                        await _service.AdicionarAsync(novoAbastecimento);
                        return "Abastecimento registrado com sucesso!";
                    }
                },
                IsEdicao ? "Atualizando abastecimento..." : "Registrando abastecimento..."
            );

            if (!string.IsNullOrEmpty(sucesso))
            {
                ShowSuccess(sucesso);
                await CarregarAbastecimentosAsync();
                LimparFormulario();
            }
        }

        private async Task RemoverAbastecimentoAsync()
        {
            if (AbastecimentoSelecionado == null)
            {
                ShowError("Nenhum abastecimento selecionado");
                return;
            }

            var sucesso = await ExecuteWithLoadingAsync(
                async () =>
                {
                    await _service.RemoverAsync(AbastecimentoSelecionado.Id);
                    return true;
                },
                "Removendo abastecimento..."
            );

            if (sucesso == true)
            {
                ShowSuccess($"Abastecimento do posto '{AbastecimentoSelecionado.Posto}' removido com sucesso!");
                await CarregarAbastecimentosAsync();
                LimparFormulario();
            }
        }

        private async Task AtualizarConsumoAsync()
        {
            if (_service == null || Abastecimentos.Count < 2)
            {
                ConsumoMensagem = "Consumo: Necessário pelo menos 2 abastecimentos";
                return;
            }

            _isCalculandoConsumo = true;

            try
            {
                var consumo = await Task.Run(() =>
                {
                    var abastecimentosList = Abastecimentos.ToList();
                    return _service.CalcularConsumoKmPorUnidade(abastecimentosList, TipoCombustivel);
                });

                if (consumo > 0)
                {
                    ConsumoMensagem = $"Consumo: {consumo:F2} km/unidade ({TipoCombustivel})";
                }
                else
                {
                    ConsumoMensagem = "Consumo: Não foi possível calcular";
                }
            }
            catch (Exception ex)
            {
                ConsumoMensagem = "Consumo: Erro no cálculo";
                _logger?.LogError(ex, "Erro ao calcular consumo");
            }
            finally
            {
                _isCalculandoConsumo = false;
            }
        }

        private void CalcularCampoFaltante(string campoAlterado)
        {
            try
            {
                switch (campoAlterado)
                {
                    case nameof(PrecoPorUnidade):
                        if (Quantidade > 0)
                            ValorTotal = Math.Round(PrecoPorUnidade * Quantidade, 2);
                        break;
                    case nameof(Quantidade):
                        if (PrecoPorUnidade > 0)
                            ValorTotal = Math.Round(PrecoPorUnidade * Quantidade, 2);
                        break;
                    case nameof(ValorTotal):
                        if (Quantidade > 0)
                            PrecoPorUnidade = Math.Round(ValorTotal / Quantidade, 2);
                        else if (PrecoPorUnidade > 0)
                            Quantidade = Math.Round(ValorTotal / PrecoPorUnidade, 2);
                        break;
                }
            }
            catch (DivideByZeroException)
            {
                ShowError("Não é possível dividir por zero");
            }
        }

        private bool ValidarDados()
        {
            ClearErrors();
            var errors = new List<string>();

            if (VeiculoId <= 0)
                errors.Add("Veículo deve ser selecionado");

            if (string.IsNullOrWhiteSpace(TipoCombustivel))
                errors.Add("Tipo de combustível é obrigatório");

            if (PrecoPorUnidade <= 0)
                errors.Add("Preço por unidade deve ser maior que zero");

            if (Quantidade <= 0)
                errors.Add("Quantidade deve ser maior que zero");

            if (QuilometragemAtual < 0)
                errors.Add("Quilometragem não pode ser negativa");

            if (string.IsNullOrWhiteSpace(Posto))
                errors.Add("Nome do posto é obrigatório");

            if (DataAbastecimento > DateTime.Now)
                errors.Add("Data do abastecimento não pode ser futura");

            if (errors.Any())
            {
                ShowError(string.Join("\n", errors));
                return false;
            }

            return true;
        }

        private void CarregarAbastecimentoParaEdicao()
        {
            if (AbastecimentoSelecionado != null)
            {
                VeiculoId = AbastecimentoSelecionado.VeiculoId;
                DataAbastecimento = AbastecimentoSelecionado.Data;
                TipoCombustivel = AbastecimentoSelecionado.TipoCombustivel;
                PrecoPorUnidade = AbastecimentoSelecionado.PrecoPorUnidade;
                Quantidade = AbastecimentoSelecionado.Quantidade;
                ValorTotal = AbastecimentoSelecionado.ValorTotal;
                QuilometragemAtual = AbastecimentoSelecionado.QuilometragemAtual;
                Posto = AbastecimentoSelecionado.Posto;
                ClearMessages();
            }
        }

        public void LimparFormulario()
        {
            VeiculoId = 0;
            DataAbastecimento = DateTime.Now;
            TipoCombustivel = string.Empty;
            PrecoPorUnidade = 0;
            Quantidade = 0;
            ValorTotal = 0;
            QuilometragemAtual = 0;
            Posto = string.Empty;
            Observacoes = string.Empty;
            AbastecimentoSelecionado = null;
            ClearMessages();
        }

        private async Task LimparFormularioAsync()
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                LimparFormulario();
            });
        }

        private async Task CalcularConsumoAsync()
        {
            await AtualizarConsumoAsync();
        }

        private async Task SelecionarAbastecimentoAsync(Abastecimento abastecimento)
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                AbastecimentoSelecionado = abastecimento;
            });
        }

        // Métodos para filtros e estatísticas
        private async Task FiltrarAbastecimentosAsync()
        {
            if (string.IsNullOrWhiteSpace(FiltroTipoCombustivel))
            {
                await CarregarAbastecimentosAsync();
                return;
            }

            var abastecimentosFiltrados = Abastecimentos
                .Where(a => a.TipoCombustivel.Contains(FiltroTipoCombustivel, StringComparison.OrdinalIgnoreCase))
                .ToList();

            Abastecimentos.Clear();
            foreach (var abastecimento in abastecimentosFiltrados.OrderByDescending(a => a.Data))
            {
                Abastecimentos.Add(abastecimento);
            }

            ShowSuccess($"{abastecimentosFiltrados.Count} abastecimentos filtrados");
        }

        // Propriedades para estatísticas
        public int TotalAbastecimentos => Abastecimentos.Count;
        public decimal GastoTotal => Abastecimentos.Sum(a => a.ValorTotal);
        public decimal QuantidadeTotal => Abastecimentos.Sum(a => a.Quantidade);
        public decimal PrecoMedio => Abastecimentos.Any() ? Abastecimentos.Average(a => a.PrecoPorUnidade) : 0;
        public string GastoTotalFormatado => GastoTotal.ToString("C2");
        public string PrecoMedioFormatado => PrecoMedio.ToString("C2");

        public Dictionary<string, int> EstatisticasPorCombustivel => 
            Abastecimentos
                .GroupBy(a => a.TipoCombustivel)
                .ToDictionary(g => g.Key, g => g.Count());

        public async Task InicializarAsync()
        {
            await CarregarAbastecimentosAsync();
            await CarregarVeiculosAsync();
        }

        public async Task CarregarVeiculosAsync()
        {
            // TODO: Implementar lógica de carregamento de veículos
            await Task.CompletedTask;
        }
    }
}
