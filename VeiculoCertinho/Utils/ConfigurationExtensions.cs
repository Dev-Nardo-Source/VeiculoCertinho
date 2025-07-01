using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;

namespace VeiculoCertinho.Utils
{
    /// <summary>
    /// Extensões para simplificar e centralizar o acesso a configurações.
    /// </summary>
    public static class ConfigurationExtensions
    {
        #region Database Configuration

        /// <summary>
        /// Obtém a connection string do banco de dados com validação.
        /// </summary>
        public static string GetDatabaseConnectionString(this IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' não encontrada ou está vazia");
            }

            return connectionString;
        }

        /// <summary>
        /// Obtém o timeout do banco de dados em segundos.
        /// </summary>
        public static int GetDatabaseTimeout(this IConfiguration configuration)
        {
            return configuration.GetValue<int>("Database:TimeoutSeconds", 30);
        }

        /// <summary>
        /// Obtém a configuração de retry do banco de dados.
        /// </summary>
        public static int GetDatabaseRetryCount(this IConfiguration configuration)
        {
            return configuration.GetValue<int>("Database:RetryCount", 3);
        }

        #endregion

        #region Selenium Configuration

        /// <summary>
        /// Obtém o timeout do Selenium em segundos.
        /// </summary>
        public static int GetSeleniumTimeout(this IConfiguration configuration)
        {
            return configuration.GetValue<int>("SeleniumSettings:TimeoutSeconds", 60);
        }

        /// <summary>
        /// Obtém o número de tentativas do Selenium.
        /// </summary>
        public static int GetSeleniumRetryCount(this IConfiguration configuration)
        {
            return configuration.GetValue<int>("SeleniumSettings:RetryCount", 3);
        }

        /// <summary>
        /// Verifica se o modo headless está habilitado.
        /// </summary>
        public static bool GetSeleniumHeadlessMode(this IConfiguration configuration)
        {
            return configuration.GetValue<bool>("SeleniumSettings:HeadlessMode", true);
        }

        /// <summary>
        /// Obtém o delay mínimo entre operações Selenium (ms).
        /// </summary>
        public static int GetSeleniumMinDelay(this IConfiguration configuration)
        {
            return configuration.GetValue<int>("SeleniumSettings:MinDelayMs", 2000);
        }

        /// <summary>
        /// Obtém o delay máximo entre operações Selenium (ms).
        /// </summary>
        public static int GetSeleniumMaxDelay(this IConfiguration configuration)
        {
            return configuration.GetValue<int>("SeleniumSettings:MaxDelayMs", 4000);
        }

        #endregion

        #region Cache Configuration

        /// <summary>
        /// Obtém o tempo padrão de expiração do cache.
        /// </summary>
        public static TimeSpan GetCacheDefaultExpiry(this IConfiguration configuration)
        {
            var minutes = configuration.GetValue<int>("Cache:DefaultExpiryMinutes", 30);
            return TimeSpan.FromMinutes(minutes);
        }

        /// <summary>
        /// Obtém o tamanho máximo do cache.
        /// </summary>
        public static int GetCacheMaxSize(this IConfiguration configuration)
        {
            return configuration.GetValue<int>("Cache:MaxSize", 10000);
        }

        /// <summary>
        /// Obtém o intervalo de limpeza do cache.
        /// </summary>
        public static TimeSpan GetCacheCleanupInterval(this IConfiguration configuration)
        {
            var minutes = configuration.GetValue<int>("Cache:CleanupIntervalMinutes", 5);
            return TimeSpan.FromMinutes(minutes);
        }

        #endregion

        #region Email Configuration

        /// <summary>
        /// Obtém a configuração de SMTP.
        /// </summary>
        public static SmtpConfiguration GetSmtpConfiguration(this IConfiguration configuration)
        {
            var section = configuration.GetSection("Email:Smtp");
            return new SmtpConfiguration
            {
                Host = section.GetValue<string>("Host", "smtp.gmail.com"),
                Port = section.GetValue<int>("Port", 587),
                EnableSsl = section.GetValue<bool>("EnableSsl", true),
                Username = section.GetValue<string>("Username", ""),
                Password = section.GetValue<string>("Password", ""),
                FromEmail = section.GetValue<string>("FromEmail", ""),
                FromName = section.GetValue<string>("FromName", "VeiculoCertinho")
            };
        }

        #endregion

        #region SMS Configuration

        /// <summary>
        /// Obtém a configuração de SMS.
        /// </summary>
        public static SmsConfiguration GetSmsConfiguration(this IConfiguration configuration)
        {
            var section = configuration.GetSection("Sms");
            return new SmsConfiguration
            {
                ApiKey = section.GetValue<string>("ApiKey", ""),
                ApiUrl = section.GetValue<string>("ApiUrl", ""),
                FromNumber = section.GetValue<string>("FromNumber", ""),
                IsEnabled = section.GetValue<bool>("IsEnabled", false)
            };
        }

        #endregion

        #region Logging Configuration

        /// <summary>
        /// Obtém o nível mínimo de log.
        /// </summary>
        public static LogLevel GetMinimumLogLevel(this IConfiguration configuration)
        {
            var levelString = configuration.GetValue<string>("Logging:LogLevel:Default", "Information");
            return Enum.TryParse<LogLevel>(levelString, true, out var level) ? level : LogLevel.Information;
        }

        /// <summary>
        /// Verifica se o log em arquivo está habilitado.
        /// </summary>
        public static bool IsFileLoggingEnabled(this IConfiguration configuration)
        {
            return configuration.GetValue<bool>("Logging:File:Enabled", true);
        }

        /// <summary>
        /// Obtém o caminho do arquivo de log.
        /// </summary>
        public static string GetLogFilePath(this IConfiguration configuration)
        {
            return configuration.GetValue<string>("Logging:File:Path", "logs/app.log");
        }

        #endregion

        #region Application Configuration

        /// <summary>
        /// Obtém a versão da aplicação.
        /// </summary>
        public static string GetApplicationVersion(this IConfiguration configuration)
        {
            return configuration.GetValue<string>("Application:Version", "1.0.0");
        }

        /// <summary>
        /// Obtém o nome da aplicação.
        /// </summary>
        public static string GetApplicationName(this IConfiguration configuration)
        {
            return configuration.GetValue<string>("Application:Name", "VeiculoCertinho");
        }

        /// <summary>
        /// Verifica se está em modo de desenvolvimento.
        /// </summary>
        public static bool IsDevelopmentMode(this IConfiguration configuration)
        {
            var environment = configuration.GetValue<string>("Environment", "Production");
            return environment.Equals("Development", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Obtém o timeout padrão para operações HTTP.
        /// </summary>
        public static TimeSpan GetHttpTimeout(this IConfiguration configuration)
        {
            var seconds = configuration.GetValue<int>("Http:TimeoutSeconds", 30);
            return TimeSpan.FromSeconds(seconds);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Obtém um valor de configuração com validação.
        /// </summary>
        public static T GetRequiredValue<T>(this IConfiguration configuration, string key)
        {
            var value = configuration.GetValue<T>(key);
            if (value == null)
            {
                throw new InvalidOperationException($"Configuração obrigatória '{key}' não encontrada");
            }
            return value;
        }

        /// <summary>
        /// Obtém um valor de configuração com validação e conversão personalizada.
        /// </summary>
        public static T GetValueWithValidation<T>(this IConfiguration configuration, string key, T defaultValue, Func<T, bool> validator)
        {
            var value = configuration.GetValue<T>(key, defaultValue);
            if (!validator(value))
            {
                throw new InvalidOperationException($"Valor inválido para configuração '{key}': {value}");
            }
            return value;
        }

        /// <summary>
        /// Obtém uma configuração como decimal de forma segura.
        /// </summary>
        public static decimal GetDecimalValue(this IConfiguration configuration, string key, decimal defaultValue = 0)
        {
            var stringValue = configuration.GetValue<string>(key);
            if (string.IsNullOrWhiteSpace(stringValue))
                return defaultValue;

            return decimal.TryParse(stringValue, NumberStyles.Number, CultureInfo.InvariantCulture, out var result)
                ? result
                : defaultValue;
        }

        #endregion
    }

    #region Configuration Models

    /// <summary>
    /// Configuração SMTP para envio de emails.
    /// </summary>
    public class SmtpConfiguration
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public bool EnableSsl { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;

        public bool IsValid => !string.IsNullOrWhiteSpace(Host) &&
                              Port > 0 &&
                              !string.IsNullOrWhiteSpace(Username) &&
                              !string.IsNullOrWhiteSpace(FromEmail);
    }

    /// <summary>
    /// Configuração para envio de SMS.
    /// </summary>
    public class SmsConfiguration
    {
        public string ApiKey { get; set; } = string.Empty;
        public string ApiUrl { get; set; } = string.Empty;
        public string FromNumber { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }

        public bool IsValid => IsEnabled &&
                              !string.IsNullOrWhiteSpace(ApiKey) &&
                              !string.IsNullOrWhiteSpace(ApiUrl);
    }

    #endregion
} 