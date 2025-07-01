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
    public class ManutencaoCorretivaViewModel : BaseViewModel
    {
        private readonly ManutencaoCorretivaService _service;
        private ManutencaoCorretiva _novaManutencao;

        public ObservableCollection<ManutencaoCorretiva> Manutencoes { get; set; } = new ObservableCollection<ManutencaoCorretiva>();

        public ManutencaoCorretiva NovaManutencao
        {
            get => _novaManutencao;
            set => SetProperty(ref _novaManutencao, value);
        }

        public ICommand RegistrarCommand { get; }

        public ManutencaoCorretivaViewModel() : this(CreateDefaultManutencaoCorretivaService())
        {
        }

        public ManutencaoCorretivaViewModel(ManutencaoCorretivaService service)
        {
            _service = service;
            _novaManutencao = new ManutencaoCorretiva
            {
                DataRegistro = DateTime.Now,
                DataResolucao = DateTime.Now
            };

            RegistrarCommand = new Command(async () => await RegistrarManutencaoAsync());

            Task.Run(async () => await CarregarManutencoesAsync());
        }

        private async Task RegistrarManutencaoAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                await AdicionarManutencaoAsync(NovaManutencao);
                NovaManutencao = new ManutencaoCorretiva
                {
                    DataRegistro = DateTime.Now,
                    DataResolucao = DateTime.Now
                };
            }
            finally
            {
                IsBusy = false;
            }
        }

        private static ManutencaoCorretivaService CreateDefaultManutencaoCorretivaService()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();
            var repositorio = new VeiculoCertinho.Repositories.ManutencaoCorretivaRepositorio(configuration, new Microsoft.Extensions.Logging.Abstractions.NullLogger<VeiculoCertinho.Repositories.ManutencaoCorretivaRepositorio>());
            var logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<ManutencaoCorretivaService>();
            var service = new ManutencaoCorretivaService(repositorio, logger);
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

        public async Task AdicionarManutencaoAsync(ManutencaoCorretiva manutencao)
        {
            await _service.AdicionarAsync(manutencao);
            Manutencoes.Add(manutencao);
            OnPropertyChanged(nameof(Manutencoes));
        }
    }
}
