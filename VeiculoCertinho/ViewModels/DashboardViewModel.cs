using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using VeiculoCertinho.Models;
using VeiculoCertinho.Services;
using VeiculoCertinho.Utils;
using Microsoft.Extensions.Logging;

namespace VeiculoCertinho.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private readonly DashboardService? _service;

        public ObservableCollection<DashboardIndicador> Indicadores { get; set; } = new ObservableCollection<DashboardIndicador>();

        // Comandos usando CommandFactory
        public ICommand AdicionarIndicadorCommand { get; private set; }
        public ICommand AtualizarCommand { get; private set; }

        // Construtor padrão necessário para compilação XAML
        public DashboardViewModel() : base(null)
        {
            _service = null;
            AdicionarIndicadorCommand = CommandFactory.Create(() => { }, () => false);
            AtualizarCommand = CommandFactory.Create(() => { }, () => false);
        }

        public DashboardViewModel(DashboardService service, ILogger<DashboardViewModel>? logger = null) : base(logger)
        {
            _service = service;
            InitializeCommands();
        }

        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            AdicionarIndicadorCommand = CommandFactory.CreateAsync<DashboardIndicador>(
                execute: AdicionarIndicadorAsync,
                canExecute: indicador => !IsBusy && indicador != null,
                onError: ex => ShowError($"Erro ao adicionar indicador: {ex.Message}"),
                logger: _logger
            );

            AtualizarCommand = CommandFactory.CreateAsyncWithLoading(
                execute: CarregarIndicadoresAsync,
                setBusy: busy => IsBusy = busy,
                canExecute: () => !IsBusy && _service != null,
                onError: ex => ShowError($"Erro ao carregar indicadores: {ex.Message}"),
                logger: _logger
            );
        }

        protected override async Task OnLoadAsync()
        {
            await CarregarIndicadoresAsync();
        }

        public async Task CarregarIndicadoresAsync()
        {
            if (_service == null)
            {
                ShowError("Serviço não disponível");
                return;
            }

            var indicadores = await ExecuteWithLoadingAsync(
                async () => await _service.ObterTodosAsync(),
                "Carregando indicadores..."
            );

            if (indicadores != null)
            {
                Indicadores.Clear();
                foreach (var indicador in indicadores)
                {
                    Indicadores.Add(indicador);
                }
                ShowSuccess($"{indicadores.Count} indicadores carregados com sucesso");
            }
        }

        public async Task AdicionarIndicadorAsync(DashboardIndicador indicador)
        {
            if (_service == null || indicador == null)
            {
                ShowError("Dados inválidos para adicionar indicador");
                return;
            }

            var sucesso = await ExecuteWithLoadingAsync(
                async () =>
                {
                    await _service.AdicionarAsync(indicador);
                    return true;
                },
                "Adicionando indicador..."
            );

            if (sucesso == true)
            {
                Indicadores.Add(indicador);
                ShowSuccess("Indicador adicionado com sucesso");
                OnPropertyChanged(nameof(Indicadores));
            }
        }

        public async Task RemoverIndicadorAsync(DashboardIndicador indicador)
        {
            if (_service == null || indicador == null)
            {
                ShowError("Dados inválidos para remover indicador");
                return;
            }

            var sucesso = await ExecuteWithLoadingAsync(
                async () =>
                {
                    await _service.RemoverAsync(indicador.Id);
                    return true;
                },
                "Removendo indicador..."
            );

            if (sucesso == true)
            {
                Indicadores.Remove(indicador);
                ShowSuccess("Indicador removido com sucesso");
                OnPropertyChanged(nameof(Indicadores));
            }
        }

        public async Task AtualizarIndicadorAsync(DashboardIndicador indicador)
        {
            if (_service == null || indicador == null)
            {
                ShowError("Dados inválidos para atualizar indicador");
                return;
            }

            var sucesso = await ExecuteWithLoadingAsync(
                async () =>
                {
                    await _service.AtualizarAsync(indicador);
                    return true;
                },
                "Atualizando indicador..."
            );

            if (sucesso == true)
            {
                ShowSuccess("Indicador atualizado com sucesso");
                OnPropertyChanged(nameof(Indicadores));
            }
        }
    }
}
