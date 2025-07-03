using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VeiculoCertinho.Repositories;
using VeiculoCertinho.Services;
using VeiculoCertinho.ViewModels;
using VeiculoCertinho.Views;
using System.Reflection;

namespace VeiculoCertinho.Utils
{
    /// <summary>
    /// Extensões para configuração automática de Dependency Injection.
    /// </summary>
    public static class DependencyInjectionExtensions
    {
        /// <summary>
        /// Registra todos os repositórios automaticamente.
        /// </summary>
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            // Interface base
            services.AddTransient<IBaseRepositorio, BaseRepositorio>();

            // Auto-registrar todos os repositórios usando reflection
            var repositoryTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Repositorio") && t != typeof(BaseRepositorio))
                .ToArray();

            foreach (var repoType in repositoryTypes)
            {
                // Registrar como Singleton com factory para configuração consistente
                services.AddSingleton(repoType, serviceProvider =>
                {
                    var config = serviceProvider.GetRequiredService<IConfiguration>();
                    var loggerType = typeof(ILogger<>).MakeGenericType(repoType);
                    var logger = serviceProvider.GetRequiredService(loggerType);
                    return Activator.CreateInstance(repoType, config, logger)!;
                });
            }

            return services;
        }

        /// <summary>
        /// Registra todos os services automaticamente.
        /// </summary>
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            // Serviços especiais com configuração customizada
            services.AddSingleton<VeiculoConsultaServiceSelenium>(sp =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                var logger = sp.GetRequiredService<ILogger<VeiculoConsultaServiceSelenium>>();
                
                int timeoutSeconds = config.GetValue<int>("PuppeteerSettings:TimeoutSeconds", 60);
                int retryCount = config.GetValue<int>("PuppeteerSettings:RetryCount", 3);
                
                return new VeiculoConsultaServiceSelenium(logger, timeoutSeconds, retryCount);
            });

            services.AddSingleton<IVeiculoConsultaServiceSelenium, VeiculoConsultaServiceSelenium>();
            services.AddSingleton<HttpClient>();

            // Auto-registrar outros services usando reflection
            var serviceTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericType &&
                           t.Name.EndsWith("Service") && 
                           t != typeof(VeiculoConsultaServiceSelenium) &&
                           t != typeof(BaseService))
                .ToArray();

            foreach (var serviceType in serviceTypes)
            {
                services.AddSingleton(serviceType);
            }

            return services;
        }

        /// <summary>
        /// Registra todos os ViewModels automaticamente.
        /// </summary>
        public static IServiceCollection AddViewModels(this IServiceCollection services)
        {
            // ViewModel especial com dependências complexas
            services.AddTransient<VeiculoViewModel>(sp =>
            {
                var consultaService = sp.GetRequiredService<IVeiculoConsultaServiceSelenium>();
                var veiculoService = sp.GetRequiredService<VeiculoService>();
                var logger = sp.GetRequiredService<ILogger<VeiculoViewModel>>();
                var ufRepositorio = sp.GetRequiredService<UfRepositorio>();
                var municipioService = sp.GetRequiredService<MunicipioService>();
                return new VeiculoViewModel(consultaService, veiculoService, logger, ufRepositorio, municipioService);
            });

            // Auto-registrar outros ViewModels usando reflection
            var viewModelTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && 
                           t.Name.EndsWith("ViewModel") && 
                           t != typeof(BaseViewModel) && 
                           t != typeof(VeiculoViewModel))
                .ToArray();

            foreach (var vmType in viewModelTypes)
            {
                services.AddTransient(vmType);
            }

            return services;
        }

        /// <summary>
        /// Registra todas as Views automaticamente.
        /// </summary>
        public static IServiceCollection AddViews(this IServiceCollection services)
        {
            // Auto-registrar todas as Pages usando reflection
            var pageTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Page"))
                .ToArray();

            foreach (var pageType in pageTypes)
            {
                services.AddTransient(pageType);
            }

            // AppShell
            services.AddTransient<AppShell>();

            return services;
        }

        /// <summary>
        /// Configura todos os componentes do VeiculoCertinho de forma automática.
        /// </summary>
        public static IServiceCollection AddVeiculoCertinho(this IServiceCollection services)
        {
            return services
                .AddRepositories()
                .AddServices()
                .AddViewModels()
                .AddViews();
        }

        /// <summary>
        /// Inicializa o banco de dados automaticamente.
        /// </summary>
        public static async Task InitializeDatabaseAsync(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            
            try
            {
                var usuarioRepositorio = scope.ServiceProvider.GetRequiredService<UsuarioRepositorio>();
                await usuarioRepositorio.InicializarBancoAsync();
            }
            catch (Exception ex)
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<object>>();
                logger.LogError(ex, "Erro ao inicializar banco de dados");
                throw;
            }
        }

        /// <summary>
        /// Método helper para registrar tipos com configuração padrão.
        /// </summary>
        private static T CreateWithConfiguration<T>(IServiceProvider serviceProvider) where T : class
        {
            var config = serviceProvider.GetRequiredService<IConfiguration>();
            var logger = serviceProvider.GetRequiredService<ILogger<T>>();
            return (T)Activator.CreateInstance(typeof(T), config, logger)!;
        }

        /// <summary>
        /// Extensão para registrar facilmente tipos que precisam de IConfiguration e ILogger.
        /// </summary>
        public static IServiceCollection AddWithConfiguration<TInterface, TImplementation>(
            this IServiceCollection services, 
            ServiceLifetime lifetime = ServiceLifetime.Singleton)
            where TInterface : class
            where TImplementation : class, TInterface
        {
            var serviceDescriptor = new ServiceDescriptor(
                typeof(TInterface),
                sp => CreateWithConfiguration<TImplementation>(sp),
                lifetime);

            services.Add(serviceDescriptor);
            return services;
        }

        /// <summary>
        /// Registra tipos por convenção baseado em sufixos.
        /// </summary>
        public static IServiceCollection AddByConvention(
            this IServiceCollection services, 
            string suffix, 
            ServiceLifetime lifetime = ServiceLifetime.Transient,
            params Type[] excludeTypes)
        {
            var types = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && 
                           t.Name.EndsWith(suffix) && 
                           !excludeTypes.Contains(t))
                .ToArray();

            foreach (var type in types)
            {
                services.Add(new ServiceDescriptor(type, type, lifetime));
            }

            return services;
        }
    }
} 