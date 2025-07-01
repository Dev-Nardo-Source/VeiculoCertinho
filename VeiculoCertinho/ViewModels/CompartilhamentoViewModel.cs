using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Linq;
using VeiculoCertinho.Models;
using VeiculoCertinho.Services;
using VeiculoCertinho.Utils;

namespace VeiculoCertinho.ViewModels
{
    /// <summary>
    /// ViewModel otimizado para compartilhamento de veículos com CommandFactory.
    /// </summary>
    public class CompartilhamentoViewModel : BaseViewModel
    {
        #region Fields

        private readonly CompartilhamentoService? _service;
        private Compartilhamento? _selectedCompartilhamento;
        private Compartilhamento? _novoCompartilhamento;

        #endregion

        #region Properties

        public ObservableCollection<Compartilhamento> Compartilhamentos { get; } = new();

        public Compartilhamento? SelectedCompartilhamento
        {
            get => _selectedCompartilhamento;
            set => SetProperty(ref _selectedCompartilhamento, value);
        }

        public Compartilhamento? NovoCompartilhamento
        {
            get => _novoCompartilhamento;
            set => SetProperty(ref _novoCompartilhamento, value);
        }

        // Computed Properties
        public bool HasCompartilhamentos => Compartilhamentos.Any();
        public int TotalCompartilhamentos => Compartilhamentos.Count;
        public bool CanRemove => SelectedCompartilhamento != null;
        public bool CanAdd => NovoCompartilhamento != null && !string.IsNullOrWhiteSpace(NovoCompartilhamento.EmailUsuario);

        #endregion

        #region Commands

        public ICommand CarregarCommand { get; private set; }
        public ICommand AdicionarCommand { get; private set; }
        public ICommand RemoverCommand { get; private set; }
        public ICommand AtualizarCommand { get; private set; }
        public ICommand NovoCommand { get; private set; }

        #endregion

        #region Constructors

        public CompartilhamentoViewModel() : this(null)
        {
        }

        public CompartilhamentoViewModel(CompartilhamentoService? service)
        {
            _service = service;

            // Configurar comandos usando CommandFactory modernizado
            CarregarCommand = CommandFactory.CreateAsync(CarregarCompartilhamentosAsync);
            AdicionarCommand = CommandFactory.CreateAsync(AdicionarCompartilhamentoAsync, () => NovoCompartilhamento != null);
            RemoverCommand = CommandFactory.CreateWithConfirmation(
                RemoverCompartilhamentoAsync,
                "Tem certeza que deseja remover este compartilhamento?",
                "Remover Compartilhamento",
                () => SelectedCompartilhamento != null
            );
            AtualizarCommand = CommandFactory.CreateAsync(CarregarCompartilhamentosAsync);
            NovoCommand = CommandFactory.CreateAsync(IniciarNovoCompartilhamentoAsync);

            // Carregar dados iniciais
            Task.Run(CarregarCompartilhamentosAsync);
        }

        #endregion

        #region Command Methods

        public async Task CarregarCompartilhamentosAsync()
        {
            if (_service == null) return;

            await ExecuteWithLoadingAsync(async () =>
            {
                var lista = await _service.ObterTodosAsync();
                
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Compartilhamentos.Clear();
                    foreach (var item in lista)
                    {
                        Compartilhamentos.Add(item);
                    }
                    UpdateComputedProperties();
                });

            }, "Carregando compartilhamentos...");
        }

        private async Task AdicionarCompartilhamentoAsync()
        {
            if (_service == null || NovoCompartilhamento == null) return;

            await ExecuteWithLoadingAsync(async () =>
            {
                await _service.AdicionarAsync(NovoCompartilhamento);
                
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Compartilhamentos.Add(NovoCompartilhamento);
                    NovoCompartilhamento = null;
                    UpdateComputedProperties();
                });

            }, "Adicionando compartilhamento...");
        }

        private async Task RemoverCompartilhamentoAsync()
        {
            if (_service == null || SelectedCompartilhamento == null) return;

            await ExecuteWithLoadingAsync(async () =>
            {
                await _service.RemoverAsync(SelectedCompartilhamento.Id);
                
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Compartilhamentos.Remove(SelectedCompartilhamento);
                    SelectedCompartilhamento = null;
                    UpdateComputedProperties();
                });

            }, "Removendo compartilhamento...");
        }

        private async Task IniciarNovoCompartilhamentoAsync()
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                NovoCompartilhamento = new Compartilhamento();
            });
        }

        #endregion

        #region Helper Methods

        private void UpdateComputedProperties()
        {
            OnPropertyChanged(nameof(HasCompartilhamentos));
            OnPropertyChanged(nameof(TotalCompartilhamentos));
            OnPropertyChanged(nameof(CanRemove));
            OnPropertyChanged(nameof(CanAdd));
        }

        #endregion
    }
}
