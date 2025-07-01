using Microsoft.Maui.Controls;
using VeiculoCertinho.ViewModels;
using VeiculoCertinho.Services;
using VeiculoCertinho.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace VeiculoCertinho.Views
{
    public partial class CompartilhamentoPage : ContentPage
    {
        private readonly CompartilhamentoViewModel _viewModel;

        public CompartilhamentoPage()
        {
            InitializeComponent();

            var configuration = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            var loggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder => { });
            var baseLogger = loggerFactory.CreateLogger("BaseLogger");

            var repositorioLogger = new VeiculoCertinho.Utils.LoggerAdapter<CompartilhamentoRepositorio>(baseLogger);
            var serviceLogger = new VeiculoCertinho.Utils.LoggerAdapter<CompartilhamentoService>(baseLogger);

            var repositorio = new CompartilhamentoRepositorio(configuration, repositorioLogger);
            var service = new CompartilhamentoService(repositorio, serviceLogger);
            _viewModel = new CompartilhamentoViewModel(service);
            BindingContext = _viewModel;
        }
    }
}
