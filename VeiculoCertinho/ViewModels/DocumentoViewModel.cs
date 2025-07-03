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
            _novoDocumento = new Documento();

            RegistrarCommand = new Command(async () => await RegistrarDocumentoAsync());

            Task.Run(async () => await CarregarDocumentosAsync());
        }

        private static DocumentoService CreateDefaultDocumentoService()
        {
            var configuration = Microsoft.Maui.Controls.Application.Current?.Handler?.MauiContext?.Services
                .GetService<IConfiguration>() ?? new ConfigurationBuilder().Build();
            
            var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<DocumentoService>.Instance;
            
            return new DocumentoService(configuration, logger);
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
            NovoDocumento = new Documento();
        }
    }
}
