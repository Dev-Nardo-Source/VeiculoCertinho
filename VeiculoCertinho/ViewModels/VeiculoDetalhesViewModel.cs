using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using VeiculoCertinho.Models;
using VeiculoCertinho.Services;
using VeiculoCertinho.Utils;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace VeiculoCertinho.ViewModels
{
    /// <summary>
    /// ViewModel otimizado para detalhes de veículos com CommandFactory e patterns modernos.
    /// </summary>
    public class VeiculoDetalhesViewModel : BaseViewModel
    {
        #region Fields

        private readonly VeiculoDetalhesService _service;
        private VeiculoDetalhes? _selectedDetalhes;
        private bool _isEditing;

        #endregion

        #region Properties

        public ObservableCollection<VeiculoDetalhes> Detalhes { get; } = new();

        public VeiculoDetalhes? SelectedDetalhes
        {
            get => _selectedDetalhes;
            set => SetProperty(ref _selectedDetalhes, value);
        }

        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }

        public bool CanEdit => SelectedDetalhes != null;
        public bool HasDetalhes => Detalhes.Any();
        public int TotalDetalhes => Detalhes.Count;

        #endregion

        #region Commands

        public ICommand CarregarCommand { get; private set; }
        public ICommand AdicionarCommand { get; private set; }
        public ICommand EditarCommand { get; private set; }
        public ICommand SalvarCommand { get; private set; }
        public ICommand CancelarCommand { get; private set; }
        public ICommand RemoverCommand { get; private set; }
        public ICommand AtualizarCommand { get; }

        #endregion

        #region Constructors

        public VeiculoDetalhesViewModel() : this(App.Current?.Handler?.MauiContext?.Services?.GetService(typeof(VeiculoDetalhesService)) as VeiculoDetalhesService)
        {
        }

        public VeiculoDetalhesViewModel(VeiculoDetalhesService? service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));

            // Configurar comandos usando CommandFactory
            CarregarCommand = CommandFactory.CreateAsync(CarregarDetalhesAsync);
            AdicionarCommand = CommandFactory.CreateAsync(IniciarNovoDetalhesAsync);
            EditarCommand = CommandFactory.CreateAsync(EditarDetalhesAsync, () => CanEdit);
            SalvarCommand = CommandFactory.CreateAsync(SalvarDetalhesAsync, () => IsEditing);
            CancelarCommand = CommandFactory.CreateAsync(CancelarEdicaoAsync, () => IsEditing);
            RemoverCommand = CommandFactory.CreateWithConfirmation(
                RemoverDetalhesAsync, 
                "Tem certeza que deseja remover este detalhe?",
                "Remover Detalhes",
                () => SelectedDetalhes != null
            );
            AtualizarCommand = CommandFactory.CreateAsync(AtualizarDetalhesAsync);

            // Carregar dados iniciais
            Task.Run(CarregarDetalhesAsync);
        }

        #endregion

        #region Command Methods

        public async Task CarregarDetalhesAsync()
        {
            await ExecuteWithLoadingAsync(async () =>
            {
                var lista = await _service.ObterTodosAsync();
                
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Detalhes.Clear();
                    foreach (var item in lista)
                    {
                        Detalhes.Add(item);
                    }
                    
                    UpdateComputedProperties();
                });

            }, "Carregando detalhes do veículo...");
        }

        public async Task AdicionarDetalhesAsync(VeiculoDetalhes detalhes)
        {
            if (detalhes == null) return;

            await ExecuteWithLoadingAsync(async () =>
            {
                await _service.AdicionarAsync(detalhes);
                
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Detalhes.Add(detalhes);
                    UpdateComputedProperties();
                });

            }, "Adicionando detalhes...");
        }

        private async Task IniciarNovoDetalhesAsync()
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                SelectedDetalhes = new VeiculoDetalhes();
                IsEditing = true;
            });
        }

        private async Task EditarDetalhesAsync()
        {
            if (SelectedDetalhes == null) return;

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                IsEditing = true;
            });
        }

        private async Task SalvarDetalhesAsync()
        {
            if (SelectedDetalhes == null) return;

            await ExecuteWithLoadingAsync(async () =>
            {
                if (SelectedDetalhes.Id == 0)
                {
                    await _service.AdicionarAsync(SelectedDetalhes);
                    await MainThread.InvokeOnMainThreadAsync(() => Detalhes.Add(SelectedDetalhes));
                }
                else
                {
                    await _service.AtualizarAsync(SelectedDetalhes);
                }

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    IsEditing = false;
                    UpdateComputedProperties();
                });

            }, "Salvando detalhes...");
        }

        private async Task CancelarEdicaoAsync()
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                if (SelectedDetalhes?.Id == 0)
                {
                    SelectedDetalhes = null;
                }
                IsEditing = false;
            });
        }

        private async Task RemoverDetalhesAsync()
        {
            if (SelectedDetalhes == null) return;

            await ExecuteWithLoadingAsync(async () =>
            {
                await _service.RemoverAsync(SelectedDetalhes.Id);
                
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Detalhes.Remove(SelectedDetalhes);
                    SelectedDetalhes = null;
                    UpdateComputedProperties();
                });

            }, "Removendo detalhes...");
        }

        private async Task AtualizarDetalhesAsync()
        {
            await CarregarDetalhesAsync();
        }

        #endregion

        #region Helper Methods

        private void UpdateComputedProperties()
        {
            OnPropertyChanged(nameof(CanEdit));
            OnPropertyChanged(nameof(HasDetalhes));
            OnPropertyChanged(nameof(TotalDetalhes));
        }

        #endregion
    }
}
