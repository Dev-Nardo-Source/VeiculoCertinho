using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using VeiculoCertinho.Repositories;
using VeiculoCertinho.Services;
using VeiculoCertinho.ViewModels;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace VeiculoCertinho;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        // Ler appsettings.json manualmente e registrar connection string diretamente
        string configFilePath = Path.Combine(FileSystem.AppDataDirectory, "appsettings.json");
        string json = "{}";
        if (File.Exists(configFilePath))
        {
            json = File.ReadAllText(configFilePath);
        }
        else
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("VeiculoCertinho.appsettings.json");
            if (stream != null)
            {
                using var reader = new StreamReader(stream);
                json = reader.ReadToEnd();
                File.WriteAllText(configFilePath, json);
            }
        }

        // Desserializar JSON para extrair connection string
        var jsonDoc = System.Text.Json.JsonDocument.Parse(json);
        string? connectionString = null;
        if (jsonDoc.RootElement.TryGetProperty("ConnectionStrings", out var connStrings))
        {
            if (connStrings.TryGetProperty("DefaultConnection", out var defaultConn))
            {
                connectionString = defaultConn.GetString();
            }
        }

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentNullException(nameof(connectionString), "A connection string 'DefaultConnection' não pode ser nula ou vazia.");
        }

        // Registrar connection string diretamente no container
        builder.Services.AddSingleton<string>(connectionString);

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Registro de repositórios para injeção de dependência
        builder.Services.AddSingleton<AbastecimentoRepositorio>(sp =>
        {
            var connectionString = sp.GetRequiredService<string>();
            var logger = sp.GetRequiredService<ILogger<AbastecimentoRepositorio>>();
            return new AbastecimentoRepositorio(connectionString, logger);
        });
        builder.Services.AddSingleton<AbastecimentoService>();

        // Registro do VeiculoRepositorio e VeiculoService
        builder.Services.AddSingleton<VeiculoRepositorio>(sp =>
        {
            var connectionString = sp.GetRequiredService<string>();
            var logger = sp.GetRequiredService<ILogger<VeiculoRepositorio>>();
            return new VeiculoRepositorio(connectionString, logger);
        });
        builder.Services.AddSingleton<VeiculoService>();

        // Registro do UsuarioRepositorio e UsuarioViewModel
        builder.Services.AddSingleton<UsuarioRepositorio>(sp =>
        {
            var connectionString = sp.GetRequiredService<string>();
            var logger = sp.GetRequiredService<ILogger<UsuarioRepositorio>>();
            return new UsuarioRepositorio(connectionString, logger);
        });
        builder.Services.AddSingleton<UsuarioViewModel>();

        // Registro do VeiculoConsultaService
        builder.Services.AddSingleton<VeiculoConsultaService>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();

        // Inicializar banco de dados
        using (var scope = app.Services.CreateScope())
        {
            var usuarioRepositorio = scope.ServiceProvider.GetRequiredService<UsuarioRepositorio>();
            usuarioRepositorio.InicializarBancoAsync().GetAwaiter().GetResult();
        }

        return app;
    }
}