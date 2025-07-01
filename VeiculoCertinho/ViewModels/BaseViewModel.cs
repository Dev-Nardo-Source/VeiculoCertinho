using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Extensions.Logging;

namespace VeiculoCertinho.ViewModels
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        protected readonly ILogger? _logger;

        protected BaseViewModel(ILogger? logger = null)
        {
            _logger = logger;
            InitializeCommands();
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        private string? _statusMessage;
        public string? StatusMessage
        {
            get => _statusMessage;
            set
            {
                SetProperty(ref _statusMessage, value);
                HasStatus = !string.IsNullOrEmpty(value);
            }
        }

        private bool _hasStatus;
        public bool HasStatus
        {
            get => _hasStatus;
            private set => SetProperty(ref _hasStatus, value);
        }

        private bool _hasErrors;
        public bool HasErrors
        {
            get => _hasErrors;
            set => SetProperty(ref _hasErrors, value);
        }

        private string? _errorMessage;
        public string? ErrorMessage
        {
            get => _errorMessage;
            set
            {
                SetProperty(ref _errorMessage, value);
                HasErrors = !string.IsNullOrEmpty(value);
            }
        }

        // Comandos base que podem ser sobrescritos
        public virtual ICommand? SaveCommand { get; protected set; }
        public virtual ICommand? CancelCommand { get; protected set; }
        public virtual ICommand? LoadCommand { get; protected set; }
        public virtual ICommand? RefreshCommand { get; protected set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        // Método helper para executar operações com loading
        protected virtual async Task ExecuteWithLoadingAsync(Func<Task> operation, string? loadingMessage = null)
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                StatusMessage = loadingMessage ?? "Carregando...";
                ClearErrors();

                await operation();
            }
            catch (Exception ex)
            {
                ShowError($"Erro: {ex.Message}");
                _logger?.LogError(ex, "Erro durante operação: {Message}", ex.Message);
            }
            finally
            {
                IsBusy = false;
                if (string.IsNullOrEmpty(ErrorMessage))
                {
                    StatusMessage = null;
                }
            }
        }

        // Método helper para executar operações com loading e retorno
        protected virtual async Task<T?> ExecuteWithLoadingAsync<T>(Func<Task<T?>> operation, string? loadingMessage = null)
        {
            if (IsBusy) return default;

            try
            {
                IsBusy = true;
                StatusMessage = loadingMessage ?? "Carregando...";
                ClearErrors();

                return await operation();
            }
            catch (Exception ex)
            {
                ShowError($"Erro: {ex.Message}");
                _logger?.LogError(ex, "Erro durante operação: {Message}", ex.Message);
                return default;
            }
            finally
            {
                IsBusy = false;
                if (string.IsNullOrEmpty(ErrorMessage))
                {
                    StatusMessage = null;
                }
            }
        }

        protected virtual void ShowError(string message)
        {
            ErrorMessage = message;
            StatusMessage = null;
        }

        protected virtual void ShowSuccess(string message)
        {
            StatusMessage = message;
            ClearErrors();
        }

        protected virtual void ClearErrors()
        {
            ErrorMessage = null;
        }

        protected virtual void ClearMessages()
        {
            StatusMessage = null;
            ErrorMessage = null;
        }

        // Factory methods para comandos
        protected ICommand CreateCommand(Action execute, Func<bool>? canExecute = null)
        {
            return new Command(execute, canExecute);
        }

        protected ICommand CreateAsyncCommand(Func<Task> execute, Func<bool>? canExecute = null)
        {
            return new Command(async () => await execute(), canExecute);
        }

        // Método virtual para inicializar comandos específicos
        protected virtual void InitializeCommands()
        {
            LoadCommand = CreateAsyncCommand(OnLoadAsync, () => !IsBusy);
            RefreshCommand = CreateAsyncCommand(OnRefreshAsync, () => !IsBusy);
        }

        // Métodos virtuais que podem ser sobrescritos
        protected virtual async Task OnLoadAsync()
        {
            await Task.CompletedTask;
        }

        protected virtual async Task OnRefreshAsync()
        {
            await OnLoadAsync();
        }

        protected virtual void OnSave()
        {
            // Override in derived classes
        }

        protected virtual void OnCancel()
        {
            ClearMessages();
        }
    }
}
