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
    public class ServicoEsteticoViewModel : BaseViewModel
    {
        private readonly ServicoEsteticoService _service;
        private ServicoEstetico _novoServico;

        public ObservableCollection<ServicoEstetico> Servicos { get; set; } = new ObservableCollection<ServicoEstetico>();

        public ServicoEstetico NovoServico
        {
            get => _novoServico;
            set => SetProperty(ref _novoServico, value);
        }

        public ICommand RegistrarCommand { get; }

        public ServicoEsteticoViewModel() : this(CreateDefaultServicoEsteticoService())
        {
        }

        public ServicoEsteticoViewModel(ServicoEsteticoService service)
        {
            _service = service;
            _novoServico = new ServicoEstetico
            {
                DataServico = DateTime.Now
            };

            RegistrarCommand = new Command(async () => await RegistrarServicoAsync());

            Task.Run(async () => await CarregarServicosAsync());
        }

        private async Task RegistrarServicoAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                await AdicionarServicoAsync(NovoServico);
                NovoServico = new ServicoEstetico
                {
                    DataServico = DateTime.Now
                };
            }
            finally
            {
                IsBusy = false;
            }
        }

        private static ServicoEsteticoService CreateDefaultServicoEsteticoService()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();
            var repositorio = new VeiculoCertinho.Repositories.ServicoEsteticoRepositorio(configuration, new Microsoft.Extensions.Logging.Abstractions.NullLogger<VeiculoCertinho.Repositories.ServicoEsteticoRepositorio>());
            var logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<ServicoEsteticoService>();
            var service = new ServicoEsteticoService(repositorio, logger);
            return service;
        }

        public async Task CarregarServicosAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                var lista = await _service.ObterTodosAsync();
                Servicos.Clear();
                foreach (var item in lista)
                {
                    Servicos.Add(item);
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task AdicionarServicoAsync(ServicoEstetico servico)
        {
            await _service.AdicionarAsync(servico);
            Servicos.Add(servico);
            OnPropertyChanged(nameof(Servicos));
        }

        public void AdicionarServico(ServicoEstetico servico)
        {
            _ = AdicionarServicoAsync(servico);
        }
    }
}
