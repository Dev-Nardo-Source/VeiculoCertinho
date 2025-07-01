using System.Threading.Tasks;
using VeiculoCertinho.Services;

namespace VeiculoCertinho.ViewModels
{
    public class SegurancaViewModel : BaseViewModel
    {
        private readonly SegurancaService? _service;

        private string? _token;
        public string? Token
        {
            get => _token;
            set
            {
                _token = value;
                OnPropertyChanged(nameof(Token));
            }
        }

        // Construtor padrão necessário para compilação XAML
        public SegurancaViewModel()
        {
            _service = null;
        }

        public SegurancaViewModel(SegurancaService service)
        {
            _service = service;
        }

        public async Task<bool> LoginAsync(string usuario, string senha)
        {
            // Ajustar para usar usuário e senha conforme a lógica real de autenticação
            if (_service == null) return false;
            Token = await _service.GerarTokenAsync(usuario);
            return !string.IsNullOrEmpty(Token);
        }

        public Task<bool> ValidarTokenAsync(string token)
        {
            if (_service == null) return Task.FromResult(false);
            return _service.ValidarTokenAsync(token);
        }

        // Adicionando método VerificarDoisFatoresAsync para corrigir erro de compilação
        public async Task<bool> VerificarDoisFatoresAsync(string codigo)
        {
            // Implementação simples para exemplo, pode ser ajustada conforme a lógica real
            if (_service == null) return false;
            return await _service.VerificarDoisFatoresAsync(codigo);
        }
    }
}
