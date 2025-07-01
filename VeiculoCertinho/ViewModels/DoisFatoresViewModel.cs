using System.Threading.Tasks;
using System.Windows.Input;
using VeiculoCertinho.Services;
using VeiculoCertinho.Utils;
using Microsoft.Extensions.Logging;

namespace VeiculoCertinho.ViewModels
{
    public class DoisFatoresViewModel : BaseViewModel
    {
        private readonly DoisFatoresService? _service;
        private string? _codigoGerado;
        private string _usuario = string.Empty;
        private string _codigoDigitado = string.Empty;
        private bool _codigoValido;

        public string Usuario
        {
            get => _usuario;
            set => SetProperty(ref _usuario, value);
        }

        public string CodigoDigitado
        {
            get => _codigoDigitado;
            set => SetProperty(ref _codigoDigitado, value);
        }

        public bool CodigoValido
        {
            get => _codigoValido;
            set => SetProperty(ref _codigoValido, value);
        }

        public string? CodigoGerado => _codigoGerado;

        // Comandos usando CommandFactory
        public ICommand GerarCodigoCommand { get; private set; }
        public ICommand ValidarCodigoCommand { get; private set; }

        // Construtor padrão necessário para compilação XAML
        public DoisFatoresViewModel() : base(null)
        {
            _service = null;
            GerarCodigoCommand = CommandFactory.Create(() => { }, () => false);
            ValidarCodigoCommand = CommandFactory.Create(() => { }, () => false);
        }

        public DoisFatoresViewModel(DoisFatoresService service, ILogger<DoisFatoresViewModel>? logger = null) : base(logger)
        {
            _service = service;
            InitializeCommands();
        }

        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            GerarCodigoCommand = CommandFactory.CreateAsyncWithLoading(
                execute: GerarCodigoInternalAsync,
                setBusy: busy => IsBusy = busy,
                canExecute: () => !IsBusy && !string.IsNullOrWhiteSpace(Usuario) && _service != null,
                onError: ex => ShowError($"Erro ao gerar código: {ex.Message}"),
                logger: _logger
            );

            ValidarCodigoCommand = CommandFactory.CreateAsyncWithLoading(
                execute: ValidarCodigoInternalAsync,
                setBusy: busy => IsBusy = busy,
                canExecute: () => !IsBusy && !string.IsNullOrWhiteSpace(CodigoDigitado) && !string.IsNullOrEmpty(_codigoGerado),
                onError: ex => ShowError($"Erro ao validar código: {ex.Message}"),
                logger: _logger
            );
        }

        public async Task<string> GerarCodigoAsync(string usuario)
        {
            if (_service == null)
            {
                ShowError("Serviço não disponível");
                return string.Empty;
            }

            if (string.IsNullOrWhiteSpace(usuario))
            {
                ShowError("Nome de usuário é obrigatório");
                return string.Empty;
            }

            Usuario = usuario;

            var codigo = await ExecuteWithLoadingAsync(
                async () => await _service.GerarCodigoAsync(usuario),
                "Gerando código de verificação..."
            );

            if (!string.IsNullOrEmpty(codigo))
            {
                _codigoGerado = codigo;
                CodigoValido = false;
                CodigoDigitado = string.Empty;
                ShowSuccess("Código gerado com sucesso! Verifique seu email/SMS.");
                OnPropertyChanged(nameof(CodigoGerado));
                return codigo;
            }

            ShowError("Falha ao gerar código de verificação");
            return string.Empty;
        }

        public async Task<bool> ValidarCodigoAsync(string usuario, string codigoRecebido)
        {
            if (_service == null || string.IsNullOrEmpty(_codigoGerado))
            {
                ShowError("Nenhum código foi gerado ou serviço não disponível");
                return false;
            }

            if (string.IsNullOrWhiteSpace(codigoRecebido))
            {
                ShowError("Código é obrigatório");
                return false;
            }

            var resultado = await ExecuteWithLoadingAsync(
                async () => await _service.ValidarCodigoAsync(usuario, codigoRecebido, _codigoGerado),
                "Validando código..."
            );

            CodigoValido = resultado ?? false;

            if (CodigoValido)
            {
                ShowSuccess("Código validado com sucesso!");
                _codigoGerado = null; // Limpar código após validação
            }
            else
            {
                ShowError("Código inválido ou expirado");
            }

            return CodigoValido;
        }

        // Métodos internos para os comandos
        private async Task GerarCodigoInternalAsync()
        {
            await GerarCodigoAsync(Usuario);
        }

        private async Task ValidarCodigoInternalAsync()
        {
            await ValidarCodigoAsync(Usuario, CodigoDigitado);
        }

        public void LimparCodigo()
        {
            _codigoGerado = null;
            CodigoDigitado = string.Empty;
            CodigoValido = false;
            ClearMessages();
            OnPropertyChanged(nameof(CodigoGerado));
        }

        public bool TemCodigoGerado => !string.IsNullOrEmpty(_codigoGerado);

        public bool PodeValidarCodigo => TemCodigoGerado && !string.IsNullOrWhiteSpace(CodigoDigitado);
    }
}
