using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using VeiculoCertinho.Models;
using VeiculoCertinho.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace VeiculoCertinho.ViewModels
{
    public class ManutencaoPreventivaViewModel : BaseViewModel
    {
        private readonly ManutencaoPreventivaService _service;
        private ManutencaoPreventiva _novaManutencao;

        public ObservableCollection<ManutencaoPreventiva> Manutencoes { get; set; } = new ObservableCollection<ManutencaoPreventiva>();

        public ManutencaoPreventiva NovaManutencao
        {
            get => _novaManutencao;
            set => SetProperty(ref _novaManutencao, value);
        }

        public ICommand AgendarCommand { get; }

        public ManutencaoPreventivaViewModel() : this(CreateDefaultManutencaoPreventivaService())
        {
        }

        public ManutencaoPreventivaViewModel(ManutencaoPreventivaService service)
        {
            _service = service;
            _novaManutencao = new ManutencaoPreventiva
            {
                DataAgendada = DateTime.Now,
                DataRealizada = null
            };

            AgendarCommand = new Command(async () => await AgendarManutencaoAsync());

            Task.Run(async () => await CarregarManutencoesAsync());
        }

        private async Task AgendarManutencaoAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                await AdicionarManutencaoAsync(NovaManutencao);
                NovaManutencao = new ManutencaoPreventiva
                {
                    DataAgendada = DateTime.Now,
                    DataRealizada = null
                };
            }
            finally
            {
                IsBusy = false;
            }
        }

        private static ManutencaoPreventivaService CreateDefaultManutencaoPreventivaService()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();
            var repositorio = new VeiculoCertinho.Repositories.ManutencaoPreventivaRepositorio(configuration, new Microsoft.Extensions.Logging.Abstractions.NullLogger<VeiculoCertinho.Repositories.ManutencaoPreventivaRepositorio>());
            var logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<ManutencaoPreventivaService>();
            var service = new ManutencaoPreventivaService(repositorio, logger);
            return service;
        }

        public async Task CarregarManutencoesAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                var lista = await _service.ObterTodosAsync();
                Manutencoes.Clear();
                foreach (var item in lista)
                {
                    Manutencoes.Add(item);
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task AdicionarManutencaoAsync(ManutencaoPreventiva manutencao)
        {
            await _service.AdicionarAsync(manutencao);
            Manutencoes.Add(manutencao);
            OnPropertyChanged(nameof(Manutencoes));
        }

        public void AdicionarManutencao(ManutencaoPreventiva manutencao)
        {
            _ = AdicionarManutencaoAsync(manutencao);
        }
    }
}
