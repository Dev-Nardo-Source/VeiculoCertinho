using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Linq;
using Microsoft.Maui.Controls;
using VeiculoCertinho.Models;
using VeiculoCertinho.Services;
using VeiculoCertinho.Utils;

namespace VeiculoCertinho.ViewModels
{
    /// <summary>
    /// ViewModel otimizado para controle de pneus com CommandFactory e validações.
    /// </summary>
    public class ControlePneusViewModel : BaseViewModel
    {
        #region Fields

        private readonly ControlePneusService? _service;
        private ControlePneus _novoPneu;
        private ControlePneus? _selectedPneu;

        #endregion

        #region Properties

        public ObservableCollection<ControlePneus> Pneus { get; } = new();

        public ControlePneus NovoPneu
        {
            get => _novoPneu;
            set => SetProperty(ref _novoPneu, value);
        }

        public ControlePneus? SelectedPneu
        {
            get => _selectedPneu;
            set => SetProperty(ref _selectedPneu, value);
        }

        // Computed Properties
        public bool HasPneus => Pneus.Any();
        public int TotalPneus => Pneus.Count;
        public bool CanRegister => NovoPneu != null;
        public bool CanRemove => SelectedPneu != null;

        #endregion

        #region Commands

        public ICommand CarregarCommand { get; private set; }
        public ICommand RegistrarCommand { get; private set; }
        public ICommand RemoverCommand { get; private set; }
        public ICommand AtualizarCommand { get; private set; }
        public ICommand LimparCommand { get; private set; }

        #endregion

        #region Constructors

        public ControlePneusViewModel() : this(null)
        {
        }

        public ControlePneusViewModel(ControlePneusService? service)
        {
            _service = service;
            _novoPneu = new ControlePneus { DataInstalacao = DateTime.Now };

            // Configurar comandos usando CommandFactory modernizado
            CarregarCommand = CommandFactory.CreateAsync(CarregarPneusAsync);
            RegistrarCommand = CommandFactory.CreateAsync(RegistrarPneuAsync, () => CanRegister);
            RemoverCommand = CommandFactory.CreateWithConfirmation(
                RemoverPneuAsync,
                "Tem certeza que deseja remover este pneu?",
                "Remover Pneu",
                () => CanRemove
            );
            AtualizarCommand = CommandFactory.CreateAsync(CarregarPneusAsync);
            LimparCommand = CommandFactory.CreateAsync(LimparFormularioAsync);

            // Carregar dados iniciais
            Task.Run(CarregarPneusAsync);
        }

        #endregion

        #region Command Methods

        public async Task CarregarPneusAsync()
        {
            if (_service == null) return;

            await ExecuteWithLoadingAsync(async () =>
            {
                var lista = await _service.ObterTodosAsync();
                
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Pneus.Clear();
                    foreach (var item in lista)
                    {
                        Pneus.Add(item);
                    }
                    UpdateComputedProperties();
                });

            }, "Carregando controle de pneus...");
        }

        private async Task RegistrarPneuAsync()
        {
            if (_service == null || !CanRegister) return;

            await ExecuteWithLoadingAsync(async () =>
            {
                await _service.AdicionarAsync(NovoPneu);
                
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Pneus.Add(NovoPneu);
                    NovoPneu = new ControlePneus { DataInstalacao = DateTime.Now };
                    UpdateComputedProperties();
                });

            }, "Registrando pneu...");
        }

        private async Task RemoverPneuAsync()
        {
            if (_service == null || SelectedPneu == null) return;

            await ExecuteWithLoadingAsync(async () =>
            {
                await _service.RemoverAsync(SelectedPneu.Id);
                
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Pneus.Remove(SelectedPneu);
                    SelectedPneu = null;
                    UpdateComputedProperties();
                });

            }, "Removendo pneu...");
        }

        private async Task LimparFormularioAsync()
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                NovoPneu = new ControlePneus { DataInstalacao = DateTime.Now };
                UpdateComputedProperties();
            });
        }

        #endregion

        #region Helper Methods

        private void UpdateComputedProperties()
        {
            OnPropertyChanged(nameof(HasPneus));
            OnPropertyChanged(nameof(TotalPneus));
            OnPropertyChanged(nameof(CanRegister));
            OnPropertyChanged(nameof(CanRemove));
        }

        /// <summary>
        /// Obtém estatísticas de pneus agrupadas por posição.
        /// </summary>
        public Dictionary<string, int> GetEstatisticasPorPosicao()
        {
            return Pneus.GroupBy(p => p.Posicao)
                        .ToDictionary(g => g.Key, g => g.Count());
        }

        /// <summary>
        /// Obtém pneus que precisam de atenção (mais de 50.000 km de uso).
        /// </summary>
        public IEnumerable<ControlePneus> GetPneusComAlerta()
        {
            return Pneus.Where(p => p.DataRemocao == null && 
                               p.QuilometragemRemocao.HasValue &&
                               (p.QuilometragemRemocao.Value - p.QuilometragemInstalacao) > 50000);
        }

        #endregion
    }
}
