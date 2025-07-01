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
    public class RelatorioViewModel : BaseViewModel
    {
        private readonly RelatorioService _service;

        public ObservableCollection<Relatorio> Relatorios { get; set; } = new ObservableCollection<Relatorio>();

        public RelatorioViewModel() : this(CreateDefaultRelatorioService())
        {
        }

        public RelatorioViewModel(RelatorioService service)
        {
            _service = service;
            Task.Run(async () => await CarregarRelatoriosAsync());
        }

        private static RelatorioService CreateDefaultRelatorioService()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();
            var repositorio = new VeiculoCertinho.Repositories.RelatorioRepositorio(configuration, new Microsoft.Extensions.Logging.Abstractions.NullLogger<VeiculoCertinho.Repositories.RelatorioRepositorio>());
            var logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<RelatorioService>();
            var service = new RelatorioService(repositorio, logger);
            return service;
        }

        public async Task CarregarRelatoriosAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                var lista = await _service.ObterTodosAsync();
                Relatorios.Clear();
                foreach (var item in lista)
                {
                    Relatorios.Add(item);
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task AdicionarRelatorioAsync(Relatorio relatorio)
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                await _service.AdicionarAsync(relatorio);
                Relatorios.Add(relatorio);
                OnPropertyChanged(nameof(Relatorios));
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
