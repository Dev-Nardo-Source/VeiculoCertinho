using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using VeiculoCertinho.Repositories;
using VeiculoCertinho.Services;
using VeiculoCertinho.ViewModels;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using Microsoft.Maui.LifecycleEvents;
using Serilog;
using Serilog.Events;
using VeiculoCertinho.Views;
using Mopups.Hosting;
using CommunityToolkit.Maui;
using VeiculoCertinho.Database;
using VeiculoCertinho.Security;
using SQLitePCL;
using VeiculoCertinho.Utils;

namespace VeiculoCertinho;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        // Inicialização essencial
        InitializeSQLite();
        InitializeDatabase();

        var builder = MauiApp.CreateBuilder();
        
        // Configurar logging
        ConfigureLogging(builder);
        
        // Configurar configuração
        var configuration = ConfigureConfiguration();
        builder.Services.AddSingleton<IConfiguration>(configuration);

        // Configurar MAUI
        ConfigureMaui(builder);

        // ✨ NOVA MAGIA: Registrar tudo automaticamente com 1 linha!
        builder.Services.AddVeiculoCertinho();

        var app = builder.Build();

        // Inicializar banco de dados
        InitializeDatabaseAsync(app.Services).GetAwaiter().GetResult();

        return app;
    }

    private static void InitializeSQLite()
    {
        raw.SetProvider(new SQLite3Provider_e_sqlite3());
        Batteries_V2.Init();
    }

    private static void InitializeDatabase()
    {
        DatabaseConfig.InitializeDatabase();
        DatabaseConfig.EnsureUfTableExists();
    }

    private static void ConfigureLogging(MauiAppBuilder builder)
    {
        var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "app.log");
        Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .WriteTo.Debug()
            .WriteTo.File(logPath, 
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(Log.Logger);
    }

    private static IConfiguration ConfigureConfiguration()
    {
#if DEBUG
        string configFilePath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
#else
        string configFilePath = Path.Combine(FileSystem.AppDataDirectory, "appsettings.json");
#endif

        // Extrair arquivo embedded se necessário
        if (!File.Exists(configFilePath))
        {
            ExtractEmbeddedConfiguration(configFilePath);
        }

        return new ConfigurationBuilder()
            .AddJsonFile(configFilePath, optional: false, reloadOnChange: true)
            .Build();
    }

    private static void ExtractEmbeddedConfiguration(string configFilePath)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("VeiculoCertinho.appsettings.json");
        if (stream != null)
        {
            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();
            File.WriteAllText(configFilePath, json);
        }
    }

    private static void ConfigureMaui(MauiAppBuilder builder)
    {
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureLifecycleEvents(events =>
            {
#if ANDROID
                events.AddAndroid(android => android.OnCreate((activity, bundle) => 
                {
                    // Otimização para Android: evitar ANR durante consultas longas
                    activity.Window?.SetFlags(
                        Android.Views.WindowManagerFlags.KeepScreenOn,
                        Android.Views.WindowManagerFlags.KeepScreenOn);
                }));
#endif
            })
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            })
            .ConfigureMopups();
    }

    private static async Task InitializeDatabaseAsync(IServiceProvider serviceProvider)
    {
        await serviceProvider.InitializeDatabaseAsync();
    }
}