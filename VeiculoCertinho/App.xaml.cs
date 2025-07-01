using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Linq;
using Microsoft.Maui.Controls.PlatformConfiguration;
using VeiculoCertinho.Views;
using VeiculoCertinho.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using VeiculoCertinho.Services;

namespace VeiculoCertinho;

	public partial class App : Application
	{
	private readonly ILogger<App> _logger;
	private readonly IServiceProvider _services;

	public App(ILogger<App> logger, IServiceProvider services)
		{
		_logger = logger;
		_services = services;
			InitializeComponent();

		// Registrar handlers de exceções não tratadas
		AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
		{
			_logger.LogError(e.ExceptionObject as Exception, "Exceção não tratada no AppDomain");
		};

		TaskScheduler.UnobservedTaskException += (sender, e) =>
		{
			_logger.LogError(e.Exception, "Exceção não tratada em Task");
			e.SetObserved();
		};

		#if WINDOWS
		Microsoft.UI.Xaml.Application.Current.UnhandledException += (sender, args) =>
		{
			HandleException(args.Exception, "Exceção não tratada na janela");
			args.Handled = true;
		};
		#endif
		}

		protected override Window CreateWindow(IActivationState? activationState)
		{
		var appShell = _services.GetRequiredService<AppShell>();
		var window = new Window(appShell)
		{
			Title = "Veículo Certinho"
		};

		// Configurar eventos da janela
		window.Created += (s, e) =>
		{
			// Inicialização quando a janela é criada
		};

		window.Activated += (s, e) =>
		{
			// Quando a janela é ativada
		};

		window.Deactivated += (s, e) =>
			{
			// Quando a janela é desativada
		};

		window.Destroying += (s, e) =>
		{
			// Limpeza quando a janela está sendo destruída
		};

			return window;
		}

	private async Task HandleExceptionAsync(Exception ex, string context)
	{
		try
		{
			// Garantir que estamos na thread principal
			if (!MainThread.IsMainThread)
			{
				MainThread.BeginInvokeOnMainThread(async () => await HandleExceptionAsync(ex, context));
				return;
			}

			// Log detalhado
			_logger.LogError(ex, "{Context}: {Message}", context, ex.Message);

			// Determinar a mensagem amigável baseada no tipo de exceção
			string userMessage = GetUserFriendlyMessage(ex);

			// Mostrar alerta para o usuário
			var window = Application.Current?.Windows.FirstOrDefault();
			if (window?.Page != null)
			{
				await window.Page.DisplayAlert(
					"Erro",
					userMessage,
					"OK"
				);
			}
			else
			{
				_logger.LogWarning("Não foi possível mostrar alerta: nenhuma janela ou página disponível");
			}
		}
		catch (Exception handlerEx)
		{
			// Se falhar ao mostrar o erro, logar e tentar mostrar um alerta básico
			_logger.LogCritical(handlerEx, "Erro ao tratar exceção: {Message}", handlerEx.Message);
			Debug.WriteLine($"Erro ao tratar exceção: {handlerEx}");
		}
	}

	private string GetUserFriendlyMessage(Exception ex)
	{
		return ex switch
		{
			HttpRequestException => "Erro de conexão com o servidor. Verifique sua conexão com a internet.",
			TimeoutException => "A operação excedeu o tempo limite. Tente novamente.",
			UnauthorizedAccessException => "Acesso não autorizado. Faça login novamente.",
			_ => "Ocorreu um erro inesperado. Tente novamente mais tarde."
		};
	}

	private void HandleException(Exception ex, string context)
	{
		Task.Run(async () => await HandleExceptionAsync(ex, context));
	}
	}
