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
    public class DocumentoViewModel : BaseViewModel
    {
        private readonly DocumentoService _service;
        private Documento _novoDocumento;

        public ObservableCollection<Documento> Documentos { get; set; } = new ObservableCollection<Documento>();

        public Documento NovoDocumento
        {
            get => _novoDocumento;
            set => SetProperty(ref _novoDocumento, value);
        }

        public ICommand RegistrarCommand { get; }

        public DocumentoViewModel() : this(CreateDefaultDocumentoService())
        {
        }

        public DocumentoViewModel(DocumentoService service)
        {
            _service = service;
            _novoDocumento = new Documento
            {
                DataRegistro = DateTime.Now
            };

            RegistrarCommand = new Command(async () => await RegistrarDocumentoAsync());

            Task.Run(async () => await CarregarDocumentosAsync());
        }

        private static DocumentoService CreateDefaultDocumentoService()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();
            var repositorio = new VeiculoCertinho.Repositories.DocumentoRepositorio(configuration, new Microsoft.Extensions.Logging.Abstractions.NullLogger<VeiculoCertinho.Repositories.DocumentoRepositorio>());
            var logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<DocumentoService>();
            var service = new DocumentoService(repositorio, logger);
            return service;
        }

        private async Task CarregarDocumentosAsync()
        {
            var lista = await _service.ObterTodosAsync();
            Documentos.Clear();
            foreach (var item in lista)
            {
                Documentos.Add(item);
            }
        }

        private async Task RegistrarDocumentoAsync()
        {
            if (NovoDocumento == null) return;

            await _service.AdicionarAsync(NovoDocumento);
            Documentos.Add(NovoDocumento);
            NovoDocumento = new Documento
            {
                DataRegistro = DateTime.Now
            };
        }
    }
}
