using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Extensions.Logging;

namespace VeiculoCertinho.Utils
{
    /// <summary>
    /// Factory para criação de comandos ICommand com padrões otimizados.
    /// Reduz código boilerplate na criação de comandos em ViewModels.
    /// </summary>
    public static class CommandFactory
    {
        /// <summary>
        /// Cria um comando síncrono simples.
        /// </summary>
        /// <param name="execute">Ação a ser executada</param>
        /// <param name="canExecute">Função que determina se o comando pode ser executado</param>
        /// <returns>Comando configurado</returns>
        public static ICommand Create(Action execute, Func<bool>? canExecute = null)
        {
            return new Command(execute, canExecute);
        }

        /// <summary>
        /// Cria um comando síncrono com parâmetro.
        /// </summary>
        /// <param name="execute">Ação a ser executada com parâmetro</param>
        /// <param name="canExecute">Função que determina se o comando pode ser executado</param>
        /// <returns>Comando configurado</returns>
        public static ICommand Create<T>(Action<T> execute, Func<T, bool>? canExecute = null)
        {
            return new Command<T>(execute, canExecute);
        }

        /// <summary>
        /// Cria um comando assíncrono com tratamento de erro automático.
        /// </summary>
        /// <param name="execute">Função assíncrona a ser executada</param>
        /// <param name="canExecute">Função que determina se o comando pode ser executado</param>
        /// <param name="onError">Ação a ser executada em caso de erro (opcional)</param>
        /// <param name="logger">Logger para registrar erros (opcional)</param>
        /// <returns>Comando configurado</returns>
        public static ICommand CreateAsync(Func<Task> execute, Func<bool>? canExecute = null, 
            Action<Exception>? onError = null, ILogger? logger = null)
        {
            return new Command(async () =>
            {
                try
                {
                    await execute();
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Erro ao executar comando assíncrono");
                    onError?.Invoke(ex);
                }
            }, canExecute);
        }

        /// <summary>
        /// Cria um comando assíncrono com parâmetro e tratamento de erro automático.
        /// </summary>
        /// <param name="execute">Função assíncrona a ser executada com parâmetro</param>
        /// <param name="canExecute">Função que determina se o comando pode ser executado</param>
        /// <param name="onError">Ação a ser executada em caso de erro (opcional)</param>
        /// <param name="logger">Logger para registrar erros (opcional)</param>
        /// <returns>Comando configurado</returns>
        public static ICommand CreateAsync<T>(Func<T, Task> execute, Func<T, bool>? canExecute = null, 
            Action<Exception>? onError = null, ILogger? logger = null)
        {
            return new Command<T>(async (parameter) =>
            {
                try
                {
                    await execute(parameter);
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Erro ao executar comando assíncrono com parâmetro: {Parameter}", parameter);
                    onError?.Invoke(ex);
                }
            }, canExecute);
        }

        /// <summary>
        /// Cria um comando assíncrono com estado de loading automático.
        /// </summary>
        /// <param name="execute">Função assíncrona a ser executada</param>
        /// <param name="setBusy">Ação para definir estado busy (true/false)</param>
        /// <param name="canExecute">Função que determina se o comando pode ser executado</param>
        /// <param name="onError">Ação a ser executada em caso de erro (opcional)</param>
        /// <param name="logger">Logger para registrar erros (opcional)</param>
        /// <returns>Comando configurado</returns>
        public static ICommand CreateAsyncWithLoading(Func<Task> execute, Action<bool> setBusy, 
            Func<bool>? canExecute = null, Action<Exception>? onError = null, ILogger? logger = null)
        {
            return new Command(async () =>
            {
                if (setBusy != null)
                {
                    try
                    {
                        setBusy(true);
                        await execute();
                    }
                    catch (Exception ex)
                    {
                        logger?.LogError(ex, "Erro ao executar comando assíncrono com loading");
                        onError?.Invoke(ex);
                    }
                    finally
                    {
                        setBusy(false);
                    }
                }
                else
                {
                    await execute();
                }
            }, canExecute);
        }

        /// <summary>
        /// Cria um comando de navegação simples.
        /// </summary>
        /// <param name="page">Página ou rota para navegar</param>
        /// <param name="canExecute">Função que determina se o comando pode ser executado</param>
        /// <param name="logger">Logger para registrar a navegação (opcional)</param>
        /// <returns>Comando configurado</returns>
        public static ICommand CreateNavigation(string page, Func<bool>? canExecute = null, ILogger? logger = null)
        {
            return CreateAsync(async () =>
            {
                logger?.LogInformation("Navegando para: {Page}", page);
                await Shell.Current.GoToAsync(page);
            }, canExecute, 
            ex => logger?.LogError(ex, "Erro ao navegar para: {Page}", page), 
            logger);
        }

        /// <summary>
        /// Cria um comando de navegação com parâmetros.
        /// </summary>
        /// <param name="page">Página ou rota para navegar</param>
        /// <param name="parameters">Parâmetros de navegação</param>
        /// <param name="canExecute">Função que determina se o comando pode ser executado</param>
        /// <param name="logger">Logger para registrar a navegação (opcional)</param>
        /// <returns>Comando configurado</returns>
        public static ICommand CreateNavigation(string page, IDictionary<string, object> parameters, 
            Func<bool>? canExecute = null, ILogger? logger = null)
        {
            return CreateAsync(async () =>
            {
                logger?.LogInformation("Navegando para: {Page} com parâmetros", page);
                await Shell.Current.GoToAsync(page, parameters);
            }, canExecute, 
            ex => logger?.LogError(ex, "Erro ao navegar para: {Page}", page), 
            logger);
        }

        /// <summary>
        /// Cria um comando de fechamento/volta.
        /// </summary>
        /// <param name="canExecute">Função que determina se o comando pode ser executado</param>
        /// <param name="logger">Logger para registrar a navegação (opcional)</param>
        /// <returns>Comando configurado</returns>
        public static ICommand CreateGoBack(Func<bool>? canExecute = null, ILogger? logger = null)
        {
            return CreateAsync(async () =>
            {
                logger?.LogInformation("Navegação: Voltando");
                await Shell.Current.GoToAsync("..");
            }, canExecute, 
            ex => logger?.LogError(ex, "Erro ao voltar na navegação"), 
            logger);
        }

        /// <summary>
        /// Cria um comando que executa uma ação após confirmação do usuário.
        /// </summary>
        /// <param name="execute">Ação a ser executada após confirmação</param>
        /// <param name="confirmationMessage">Mensagem de confirmação</param>
        /// <param name="confirmationTitle">Título da confirmação</param>
        /// <param name="canExecute">Função que determina se o comando pode ser executado</param>
        /// <param name="logger">Logger (opcional)</param>
        /// <returns>Comando configurado</returns>
        public static ICommand CreateWithConfirmation(Func<Task> execute, string confirmationMessage, 
            string confirmationTitle = "Confirmação", Func<bool>? canExecute = null, ILogger? logger = null)
        {
            return CreateAsync(async () =>
            {
                var result = await (Application.Current?.MainPage?.DisplayAlert(
                    confirmationTitle, confirmationMessage, "Sim", "Não") ?? Task.FromResult(false));
                
                if (result)
                {
                    logger?.LogInformation("Usuário confirmou ação: {Message}", confirmationMessage);
                    await execute();
                }
                else
                {
                    logger?.LogInformation("Usuário cancelou ação: {Message}", confirmationMessage);
                }
            }, canExecute, 
            ex => logger?.LogError(ex, "Erro ao executar ação com confirmação"), 
            logger);
        }

        /// <summary>
        /// Cria um comando com retry automático em caso de falha.
        /// </summary>
        /// <param name="execute">Função a ser executada</param>
        /// <param name="maxRetries">Número máximo de tentativas</param>
        /// <param name="retryDelay">Delay entre tentativas (em milissegundos)</param>
        /// <param name="canExecute">Função que determina se o comando pode ser executado</param>
        /// <param name="logger">Logger (opcional)</param>
        /// <returns>Comando configurado</returns>
        public static ICommand CreateWithRetry(Func<Task> execute, int maxRetries = 3, int retryDelay = 1000, 
            Func<bool>? canExecute = null, ILogger? logger = null)
        {
            return CreateAsync(async () =>
            {
                var attempt = 1;
                Exception? lastException = null;

                while (attempt <= maxRetries)
                {
                    try
                    {
                        logger?.LogInformation("Tentativa {Attempt}/{MaxRetries}", attempt, maxRetries);
                        await execute();
                        return; // Sucesso, sair do loop
                    }
                    catch (Exception ex)
                    {
                        lastException = ex;
                        logger?.LogWarning("Falha na tentativa {Attempt}/{MaxRetries}: {Error}", 
                            attempt, maxRetries, ex.Message);

                        if (attempt < maxRetries)
                        {
                            await Task.Delay(retryDelay * attempt); // Backoff exponencial
                        }

                        attempt++;
                    }
                }

                // Se chegou aqui, todas as tentativas falharam
                logger?.LogError(lastException, "Todas as {MaxRetries} tentativas falharam", maxRetries);
                throw lastException!;
            }, canExecute, 
            ex => logger?.LogError(ex, "Erro final após {MaxRetries} tentativas", maxRetries), 
            logger);
        }
    }
} 