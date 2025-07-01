using System;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using VeiculoCertinho.Models;
using VeiculoCertinho.Repositories;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using VeiculoCertinho.Utils;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace VeiculoCertinho.ViewModels
{
    public class UsuarioViewModel : BaseViewModel
    {
        private readonly UsuarioRepositorio _usuarioRepositorio;
        
        // Propriedades de entrada
        private string _nome = string.Empty;
        private string _email = string.Empty;
        private string _senha = string.Empty;
        private string _confirmarSenha = string.Empty;
        private string _perfil = "Usuario";
        private Usuario? _usuarioSelecionado;

        // Coleções
        public ObservableCollection<Usuario> Usuarios { get; private set; } = new ObservableCollection<Usuario>();

        [Required(ErrorMessage = "Nome é obrigatório")]
        [MinLength(3, ErrorMessage = "Nome deve ter pelo menos 3 caracteres")]
        [MaxLength(100, ErrorMessage = "Nome deve ter no máximo 100 caracteres")]
        public string Nome
        {
            get => _nome;
            set => SetProperty(ref _nome, value);
        }

        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email deve ter um formato válido")]
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value?.ToLowerInvariant());
        }

        [Required(ErrorMessage = "Senha é obrigatória")]
        [MinLength(6, ErrorMessage = "Senha deve ter pelo menos 6 caracteres")]
        public string Senha
        {
            get => _senha;
            set => SetProperty(ref _senha, value);
        }

        [Required(ErrorMessage = "Confirmação de senha é obrigatória")]
        public string ConfirmarSenha
        {
            get => _confirmarSenha;
            set => SetProperty(ref _confirmarSenha, value);
        }

        [Required(ErrorMessage = "Perfil é obrigatório")]
        public string Perfil
        {
            get => _perfil;
            set => SetProperty(ref _perfil, value);
        }

        public Usuario? UsuarioSelecionado
        {
            get => _usuarioSelecionado;
            set
            {
                if (SetProperty(ref _usuarioSelecionado, value))
                {
                    CarregarUsuarioParaEdicao();
                }
            }
        }

        // Propriedades calculadas
        public bool CanSalvar => !IsBusy && !string.IsNullOrWhiteSpace(Nome) && !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Senha);
        public bool CanAtualizar => !IsBusy && UsuarioSelecionado != null && CanSalvar;
        public bool CanRemover => !IsBusy && UsuarioSelecionado != null;
        public bool IsEdicao => UsuarioSelecionado != null;
        public string TituloFormulario => IsEdicao ? "Editar Usuário" : "Cadastrar Usuário";
        public string TextoBotaoSalvar => IsEdicao ? "Atualizar" : "Cadastrar";

        // Lista de perfis disponíveis
        public List<string> PerfisDisponiveis { get; } = new List<string> { "Usuario", "Admin", "Moderador" };

        // Comandos usando CommandFactory
        public ICommand SalvarCommand { get; private set; }
        public ICommand RemoverCommand { get; private set; }
        public ICommand LimparCommand { get; private set; }
        public ICommand ValidarLoginCommand { get; private set; }
        public ICommand SelecionarUsuarioCommand { get; private set; }

        // Construtor padrão para XAML
        public UsuarioViewModel() : base(null)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            _usuarioRepositorio = new UsuarioRepositorio(configuration, NullLogger<UsuarioRepositorio>.Instance);
            InitializeCommands();
        }

        public UsuarioViewModel(IConfiguration configuration, ILogger<UsuarioViewModel>? logger = null) : base(logger)
        {
            _usuarioRepositorio = new UsuarioRepositorio(configuration, NullLogger<UsuarioRepositorio>.Instance);
            InitializeCommands();
        }

        private void InitializeCommands()
        {
            SalvarCommand = CommandFactory.CreateAsync(SalvarUsuarioAsync, () => CanSalvar);

            RemoverCommand = CommandFactory.CreateWithConfirmation(
                RemoverUsuarioAsync,
                "Tem certeza que deseja remover este usuário?",
                "Remover Usuário",
                () => UsuarioSelecionado != null
            );

            LimparCommand = CommandFactory.CreateAsync(LimparFormularioAsync);

            ValidarLoginCommand = CommandFactory.CreateAsync(ValidarCredenciaisAsync, () => !string.IsNullOrWhiteSpace(Email));

            SelecionarUsuarioCommand = CommandFactory.CreateAsync<Usuario>(
                usuario => SelecionarUsuarioAsync(usuario));
        }

        protected override async Task OnLoadAsync()
        {
            await CarregarUsuariosAsync();
        }

        public async Task CarregarUsuariosAsync()
        {
            var usuarios = await ExecuteWithLoadingAsync(
                async () => await _usuarioRepositorio.ObterTodosAsync(),
                "Carregando usuários..."
            );

            if (usuarios != null)
            {
                Usuarios.Clear();
                foreach (var usuario in usuarios.OrderBy(u => u.Nome))
                {
                    Usuarios.Add(usuario);
                }
                ShowSuccess($"{usuarios.Count} usuários carregados");
            }
        }

        private async Task SalvarUsuarioAsync()
        {
            // Validar dados antes de salvar
            if (!ValidarDados())
            {
                return;
            }

            var sucesso = await ExecuteWithLoadingAsync(
                async () =>
                {
                    if (IsEdicao)
                    {
                        // Atualizar usuário existente
                        UsuarioSelecionado!.Nome = Nome;
                        UsuarioSelecionado.Email = Email;
                        UsuarioSelecionado.Senha = Senha;
                        UsuarioSelecionado.Perfil = Perfil;

                        await _usuarioRepositorio.AtualizarAsync(UsuarioSelecionado);
                        return "Usuário atualizado com sucesso!";
                    }
                    else
                    {
                        // Criar novo usuário
                        var novoUsuario = new Usuario
                        {
                            Nome = Nome,
                            Email = Email,
                            Senha = Senha,
                            Perfil = Perfil
                        };

                        await _usuarioRepositorio.AdicionarAsync(novoUsuario);
                        return "Usuário cadastrado com sucesso!";
                    }
                },
                IsEdicao ? "Atualizando usuário..." : "Cadastrando usuário..."
            );

            if (!string.IsNullOrEmpty(sucesso))
            {
                ShowSuccess(sucesso);
                await CarregarUsuariosAsync();
                LimparFormulario();
            }
        }

        private async Task RemoverUsuarioAsync()
        {
            if (UsuarioSelecionado == null)
            {
                ShowError("Nenhum usuário selecionado");
                return;
            }

            var sucesso = await ExecuteWithLoadingAsync(
                async () =>
                {
                    await _usuarioRepositorio.RemoverAsync(UsuarioSelecionado.Id);
                    return true;
                },
                "Removendo usuário..."
            );

            if (sucesso == true)
            {
                ShowSuccess($"Usuário '{UsuarioSelecionado.Nome}' removido com sucesso!");
                await CarregarUsuariosAsync();
                LimparFormulario();
            }
        }

        public async Task<Usuario?> ValidarLoginAsync(string email, string senha)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha))
            {
                ShowError("Email e senha são obrigatórios");
                return null;
            }

            var usuario = await ExecuteWithLoadingAsync(
                async () =>
                {
                    var user = await _usuarioRepositorio.ObterPorEmailAsync(email);
                    
                    if (user != null && user.Senha == senha)
                    {
                        // Atualizar último login
                        user.RegistrarLogin();
                        await _usuarioRepositorio.AtualizarAsync(user);
                        return user;
                    }

                    return null;
                },
                "Validando credenciais..."
            );

            if (usuario != null)
            {
                ShowSuccess($"Login válido! Bem-vindo(a), {usuario.Nome}");
                return usuario;
            }
            else
            {
                ShowError("Email ou senha incorretos");
                return null;
            }
        }

        private bool ValidarDados()
        {
            ClearErrors();
            var errors = new List<string>();

            // Validar nome
            if (string.IsNullOrWhiteSpace(Nome))
                errors.Add("Nome é obrigatório");
            else if (Nome.Length < 3)
                errors.Add("Nome deve ter pelo menos 3 caracteres");
            else if (Nome.Length > 100)
                errors.Add("Nome deve ter no máximo 100 caracteres");

            // Validar email
            if (string.IsNullOrWhiteSpace(Email))
                errors.Add("Email é obrigatório");
            else if (!IsValidEmail(Email))
                errors.Add("Email deve ter um formato válido");

            // Validar senha
            if (string.IsNullOrWhiteSpace(Senha))
                errors.Add("Senha é obrigatória");
            else if (Senha.Length < 6)
                errors.Add("Senha deve ter pelo menos 6 caracteres");

            // Validar confirmação de senha
            if (Senha != ConfirmarSenha)
                errors.Add("As senhas não conferem");

            // Validar perfil
            if (string.IsNullOrWhiteSpace(Perfil) || !PerfisDisponiveis.Contains(Perfil))
                errors.Add("Perfil deve ser selecionado da lista");

            if (errors.Any())
            {
                ShowError(string.Join("\n", errors));
                return false;
            }

            return true;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private void CarregarUsuarioParaEdicao()
        {
            if (UsuarioSelecionado != null)
            {
                Nome = UsuarioSelecionado.Nome;
                Email = UsuarioSelecionado.Email;
                Senha = string.Empty; // Por segurança, não carregar senha
                ConfirmarSenha = string.Empty;
                Perfil = UsuarioSelecionado.Perfil;
                ClearMessages();
            }
        }

        public void LimparFormulario()
        {
            Nome = string.Empty;
            Email = string.Empty;
            Senha = string.Empty;
            ConfirmarSenha = string.Empty;
            Perfil = "Usuario";
            UsuarioSelecionado = null;
            ClearMessages();
        }

        private async Task LimparFormularioAsync()
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                LimparFormulario();
            });
        }

        private async Task ValidarCredenciaisAsync()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                ShowError("Digite um email para validar");
                return;
            }

            var usuario = await _usuarioRepositorio.ObterPorEmailAsync(Email);
            if (usuario != null)
            {
                ShowSuccess("Email encontrado no sistema");
            }
            else
            {
                ShowError("Email não encontrado");
            }
        }

        private async Task SelecionarUsuarioAsync(Usuario usuario)
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                UsuarioSelecionado = usuario;
            });
        }

        // Propriedades para estatísticas
        public int TotalUsuarios => Usuarios.Count;
        public int UsuariosAdmin => Usuarios.Count(u => u.IsAdmin);
        public int UsuariosAtivos => Usuarios.Count(u => u.EstaAtivo);

        // Métodos para filtros/pesquisa
        public async Task PesquisarUsuariosAsync(string termo)
        {
            if (string.IsNullOrWhiteSpace(termo))
            {
                await CarregarUsuariosAsync();
                return;
            }

            var usuariosFiltrados = Usuarios
                .Where(u => u.Nome.Contains(termo, StringComparison.OrdinalIgnoreCase) ||
                           u.Email.Contains(termo, StringComparison.OrdinalIgnoreCase))
                .ToList();

            Usuarios.Clear();
            foreach (var usuario in usuariosFiltrados)
            {
                Usuarios.Add(usuario);
            }

            ShowSuccess($"{usuariosFiltrados.Count} usuários encontrados");
        }
    }
}
