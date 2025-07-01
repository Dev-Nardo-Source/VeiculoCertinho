using System.Threading.Tasks;
using System.Windows.Input;
using VeiculoCertinho.Services;
using VeiculoCertinho.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Authentication;

namespace VeiculoCertinho.ViewModels
{
    /// <summary>
    /// ViewModel consolidado para integração com serviços externos (emails, SMS, órgãos públicos).
    /// Substitui IntegracaoViewModel e IntegracaoOrgaosViewModel.
    /// </summary>
    public class IntegracaoViewModel : BaseViewModel
    {
        #region Services

        private readonly IntegracaoService? _integracaoService;
        private readonly IntegracaoOrgaosService? _orgaosService;

        #endregion

        #region Properties

        private string _destinatarioEmail = string.Empty;
        private string _assuntoEmail = string.Empty;
        private string _corpoEmail = string.Empty;
        private string _numeroSms = string.Empty;
        private string _mensagemSms = string.Empty;
        private string _placaConsulta = string.Empty;
        private string _resultadoConsulta = string.Empty;
        private byte[]? _documentoDownload;

        public string DestinatarioEmail
        {
            get => _destinatarioEmail;
            set => SetProperty(ref _destinatarioEmail, value);
        }

        public string AssuntoEmail
        {
            get => _assuntoEmail;
            set => SetProperty(ref _assuntoEmail, value);
        }

        public string CorpoEmail
        {
            get => _corpoEmail;
            set => SetProperty(ref _corpoEmail, value);
        }

        public string NumeroSms
        {
            get => _numeroSms;
            set => SetProperty(ref _numeroSms, value);
        }

        public string MensagemSms
        {
            get => _mensagemSms;
            set => SetProperty(ref _mensagemSms, value);
        }

        public string PlacaConsulta
        {
            get => _placaConsulta;
            set => SetProperty(ref _placaConsulta, value);
        }

        public string ResultadoConsulta
        {
            get => _resultadoConsulta;
            set => SetProperty(ref _resultadoConsulta, value);
        }

        public byte[]? DocumentoDownload
        {
            get => _documentoDownload;
            set => SetProperty(ref _documentoDownload, value);
        }

        // Computed Properties
        public bool CanSendEmail => !string.IsNullOrWhiteSpace(DestinatarioEmail) && 
                                   !string.IsNullOrWhiteSpace(AssuntoEmail) && 
                                   _integracaoService != null;

        public bool CanSendSms => !string.IsNullOrWhiteSpace(NumeroSms) && 
                                 !string.IsNullOrWhiteSpace(MensagemSms) && 
                                 _integracaoService != null;

        public bool CanConsultarMultas => !string.IsNullOrWhiteSpace(PlacaConsulta) && 
                                         _orgaosService != null;

        public bool CanDownloadCRLVe => !string.IsNullOrWhiteSpace(PlacaConsulta) && 
                                       _orgaosService != null;

        public bool HasDocument => DocumentoDownload != null && DocumentoDownload.Length > 0;

        #endregion

        #region Commands

        public ICommand EnviarEmailCommand { get; private set; }
        public ICommand EnviarSmsCommand { get; private set; }
        public ICommand ConsultarMultasCommand { get; private set; }
        public ICommand DownloadCRLVeCommand { get; private set; }
        public ICommand LimparFormularioCommand { get; private set; }
        public ICommand SalvarDocumentoCommand { get; private set; }

        #endregion

        #region Constructors

        public IntegracaoViewModel() : this(null, null)
        {
        }

        public IntegracaoViewModel(IntegracaoService? integracaoService, IntegracaoOrgaosService? orgaosService)
        {
            _integracaoService = integracaoService;
            _orgaosService = orgaosService;

            // Configurar comandos usando CommandFactory correto
            EnviarEmailCommand = CommandFactory.CreateAsync(EnviarEmailAsync, () => CanSendEmail);
            EnviarSmsCommand = CommandFactory.CreateAsync(EnviarSmsAsync, () => CanSendSms);
            ConsultarMultasCommand = CommandFactory.CreateAsync(ConsultarMultasAsync, () => CanConsultarMultas);
            DownloadCRLVeCommand = CommandFactory.CreateAsync(DownloadCRLVeAsync, () => CanDownloadCRLVe);
            LimparFormularioCommand = CommandFactory.CreateAsync(LimparFormularioAsync);
            SalvarDocumentoCommand = CommandFactory.CreateAsync(SalvarDocumentoAsync, () => HasDocument);
        }

        #endregion

        #region Email Methods

        public async Task EnviarEmailAsync()
        {
            if (_integracaoService == null || !CanSendEmail) return;

            await ExecuteWithLoadingAsync(async () =>
            {
                await _integracaoService.EnviarEmailAsync(DestinatarioEmail, AssuntoEmail, CorpoEmail);
                
                // Limpar campos após sucesso
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    DestinatarioEmail = string.Empty;
                    AssuntoEmail = string.Empty;
                    CorpoEmail = string.Empty;
                    UpdateComputedProperties();
                });

            }, "Enviando email...");
        }

        public async Task EnviarEmailAsync(string destinatario, string assunto, string corpo)
        {
            if (_integracaoService == null) return;

            await ExecuteWithLoadingAsync(async () =>
            {
                await _integracaoService.EnviarEmailAsync(destinatario, assunto, corpo);
            }, "Enviando email...");
        }

        #endregion

        #region SMS Methods

        public async Task EnviarSmsAsync()
        {
            if (_integracaoService == null || !CanSendSms) return;

            await ExecuteWithLoadingAsync(async () =>
            {
                await _integracaoService.EnviarSmsAsync(NumeroSms, MensagemSms);
                
                // Limpar campos após sucesso
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    NumeroSms = string.Empty;
                    MensagemSms = string.Empty;
                    UpdateComputedProperties();
                });

            }, "Enviando SMS...");
        }

        public async Task EnviarSmsAsync(string numero, string mensagem)
        {
            if (_integracaoService == null) return;

            await ExecuteWithLoadingAsync(async () =>
            {
                await _integracaoService.EnviarSmsAsync(numero, mensagem);
            }, "Enviando SMS...");
        }

        #endregion

        #region Órgãos Públicos Methods

        public async Task ConsultarMultasAsync()
        {
            if (_orgaosService == null || !CanConsultarMultas) return;

            await ExecuteWithLoadingAsync(async () =>
            {
                var resultado = await _orgaosService.ConsultarMultasAsync(PlacaConsulta);
                
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    ResultadoConsulta = resultado;
                });

            }, "Consultando multas...");
        }

        public async Task<string> ConsultarMultasAsync(string placa)
        {
            if (_orgaosService == null) return string.Empty;

            return await ExecuteWithLoadingAsync(async () =>
            {
                return await _orgaosService.ConsultarMultasAsync(placa);
            }, "Consultando multas...");
        }

        public async Task DownloadCRLVeAsync()
        {
            if (_orgaosService == null || !CanDownloadCRLVe) return;

            await ExecuteWithLoadingAsync(async () =>
            {
                var documento = await _orgaosService.DownloadCRLVeAsync(PlacaConsulta);
                
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    DocumentoDownload = documento;
                    UpdateComputedProperties();
                });

            }, "Baixando CRLV-e...");
        }

        public async Task<byte[]> DownloadCRLVeAsync(string placa)
        {
            if (_orgaosService == null) return Array.Empty<byte>();

            return await ExecuteWithLoadingAsync(async () =>
            {
                return await _orgaosService.DownloadCRLVeAsync(placa);
            }, "Baixando CRLV-e...");
        }

        #endregion

        #region Utility Methods

        public async Task LimparFormularioAsync()
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                DestinatarioEmail = string.Empty;
                AssuntoEmail = string.Empty;
                CorpoEmail = string.Empty;
                NumeroSms = string.Empty;
                MensagemSms = string.Empty;
                PlacaConsulta = string.Empty;
                ResultadoConsulta = string.Empty;
                DocumentoDownload = null;
                UpdateComputedProperties();
            });
        }

        private async Task SalvarDocumentoAsync()
        {
            if (DocumentoDownload == null || DocumentoDownload.Length == 0) return;

            await ExecuteWithLoadingAsync(async () =>
            {
                var fileName = $"CRLV_{PlacaConsulta}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                // TODO: Implementar salvamento do documento quando os serviços adequados estiverem disponíveis
                ShowSuccess($"Documento {fileName} preparado para salvamento ({DocumentoDownload.Length} bytes)");
                
                await Task.CompletedTask; // Placeholder para operação assíncrona

            }, "Preparando documento...");
        }

        private void UpdateComputedProperties()
        {
            OnPropertyChanged(nameof(CanSendEmail));
            OnPropertyChanged(nameof(CanSendSms));
            OnPropertyChanged(nameof(CanConsultarMultas));
            OnPropertyChanged(nameof(CanDownloadCRLVe));
            OnPropertyChanged(nameof(HasDocument));
        }

        #endregion
    }
}
