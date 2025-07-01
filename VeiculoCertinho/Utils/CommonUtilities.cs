using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Globalization;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;

namespace VeiculoCertinho.Utils
{
    /// <summary>
    /// Utilitários comuns consolidados para eliminação de arquivos redundantes.
    /// Substitui: Helpers.cs, DatabaseHelper.cs, DatabaseCleaner.cs, LoggerAdapter.cs
    /// </summary>
    public static class CommonUtilities
    {
        #region Database Utilities

        private const string DatabaseFile = "veiculocertinho.db";

        /// <summary>
        /// Remove uma tabela específica do banco de dados.
        /// </summary>
        public static void DropTable(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Nome da tabela não pode ser vazio", nameof(tableName));

            if (!File.Exists(DatabaseFile))
            {
                Console.WriteLine($"Banco de dados não encontrado: {DatabaseFile}");
                return;
            }

            using var connection = new SqliteConnection($"Data Source={DatabaseFile}");
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = $"DROP TABLE IF EXISTS {tableName};";
            command.ExecuteNonQuery();

            Console.WriteLine($"Tabela {tableName} removida com sucesso.");
        }

        /// <summary>
        /// Remove a tabela Veiculo (mantido para compatibilidade).
        /// </summary>
        public static void DropVeiculoTable() => DropTable("Veiculo");

        /// <summary>
        /// Limpa a tabela Veiculo (mantido para compatibilidade).
        /// </summary>
        public static void LimparTabelaVeiculo() => DropTable("Veiculo");

        /// <summary>
        /// Verifica se uma tabela existe no banco de dados.
        /// </summary>
        public static bool TableExists(string tableName)
        {
            if (!File.Exists(DatabaseFile)) return false;

            using var connection = new SqliteConnection($"Data Source={DatabaseFile}");
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name=@tableName;";
            command.Parameters.AddWithValue("@tableName", tableName);

            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }

        /// <summary>
        /// Obtém informações sobre todas as tabelas do banco.
        /// </summary>
        public static List<string> GetAllTables()
        {
            var tables = new List<string>();

            if (!File.Exists(DatabaseFile)) return tables;

            using var connection = new SqliteConnection($"Data Source={DatabaseFile}");
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                tables.Add(reader.GetString(0));
            }

            return tables;
        }

        #endregion

        #region Resource Utilities

        /// <summary>
        /// Lê um recurso incorporado do assembly.
        /// </summary>
        public static string ReadEmbeddedResource(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(resourceName);
            
            if (stream == null)
                throw new FileNotFoundException($"Recurso incorporado não encontrado: {resourceName}");
            
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        /// <summary>
        /// Obtém todos os recursos incorporados do assembly.
        /// </summary>
        public static string[] GetEmbeddedResourceNames()
        {
            var assembly = Assembly.GetExecutingAssembly();
            return assembly.GetManifestResourceNames();
        }

        /// <summary>
        /// Extrai um recurso incorporado para um arquivo no disco.
        /// </summary>
        public static async Task ExtractEmbeddedResourceToFileAsync(string resourceName, string targetFilePath)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(resourceName);
            
            if (stream == null)
                throw new FileNotFoundException($"Recurso incorporado não encontrado: {resourceName}");

            Directory.CreateDirectory(Path.GetDirectoryName(targetFilePath)!);
            
            using var fileStream = File.Create(targetFilePath);
            await stream.CopyToAsync(fileStream);
        }

        #endregion

        #region String Utilities

        /// <summary>
        /// Formata uma placa de veículo no padrão brasileiro.
        /// </summary>
        public static string FormatPlaca(string placa)
        {
            if (string.IsNullOrWhiteSpace(placa)) return string.Empty;

            var cleanPlaca = Regex.Replace(placa, @"[^\w]", "").ToUpper();
            
            if (cleanPlaca.Length != 7) return placa; // Retorna original se inválida

            // Formato: ABC-1234 ou ABC1D23 (Mercosul)
            if (char.IsLetter(cleanPlaca[4]))
            {
                // Mercosul: ABC1D23
                return $"{cleanPlaca.Substring(0, 3)}{cleanPlaca.Substring(3, 1)}{cleanPlaca.Substring(4, 1)}{cleanPlaca.Substring(5, 2)}";
            }
            else
            {
                // Padrão: ABC-1234
                return $"{cleanPlaca.Substring(0, 3)}-{cleanPlaca.Substring(3, 4)}";
            }
        }

        /// <summary>
        /// Valida se uma placa está no formato correto.
        /// </summary>
        public static bool IsValidPlaca(string placa)
        {
            if (string.IsNullOrWhiteSpace(placa)) return false;

            var cleanPlaca = Regex.Replace(placa, @"[^\w]", "").ToUpper();
            
            if (cleanPlaca.Length != 7) return false;

            // Padrão antigo: 3 letras + 4 números
            var padraoAntigo = Regex.IsMatch(cleanPlaca, @"^[A-Z]{3}\d{4}$");
            
            // Padrão Mercosul: 3 letras + número + letra + 2 números
            var padraoMercosul = Regex.IsMatch(cleanPlaca, @"^[A-Z]{3}\d[A-Z]\d{2}$");

            return padraoAntigo || padraoMercosul;
        }

        /// <summary>
        /// Formata um CPF ou CNPJ.
        /// </summary>
        public static string FormatCpfCnpj(string documento)
        {
            if (string.IsNullOrWhiteSpace(documento)) return string.Empty;

            var cleanDoc = Regex.Replace(documento, @"\D", "");

            return cleanDoc.Length switch
            {
                11 => $"{cleanDoc.Substring(0, 3)}.{cleanDoc.Substring(3, 3)}.{cleanDoc.Substring(6, 3)}-{cleanDoc.Substring(9, 2)}",
                14 => $"{cleanDoc.Substring(0, 2)}.{cleanDoc.Substring(2, 3)}.{cleanDoc.Substring(5, 3)}/{cleanDoc.Substring(8, 4)}-{cleanDoc.Substring(12, 2)}",
                _ => documento
            };
        }

        /// <summary>
        /// Remove acentos de uma string.
        /// </summary>
        public static string RemoveAccents(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            var normalizedString = input.Normalize(NormalizationForm.FormD);
            var stringBuilder = new System.Text.StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        /// <summary>
        /// Converte string para formato Title Case.
        /// </summary>
        public static string ToTitleCase(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            var textInfo = CultureInfo.CurrentCulture.TextInfo;
            return textInfo.ToTitleCase(input.ToLower());
        }

        #endregion

        #region Numeric Utilities

        /// <summary>
        /// Converte string para decimal de forma segura.
        /// </summary>
        public static decimal SafeParseDecimal(string value, decimal defaultValue = 0)
        {
            if (string.IsNullOrWhiteSpace(value)) return defaultValue;

            // Remove caracteres não numéricos exceto vírgula, ponto e sinais
            var cleanValue = Regex.Replace(value, @"[^\d.,-]", "");
            
            // Substitui vírgula por ponto para parsing
            cleanValue = cleanValue.Replace(',', '.');

            return decimal.TryParse(cleanValue, NumberStyles.Number, CultureInfo.InvariantCulture, out var result) 
                ? result 
                : defaultValue;
        }

        /// <summary>
        /// Converte string para int de forma segura.
        /// </summary>
        public static int SafeParseInt(string value, int defaultValue = 0)
        {
            if (string.IsNullOrWhiteSpace(value)) return defaultValue;

            var cleanValue = Regex.Replace(value, @"\D", "");
            return int.TryParse(cleanValue, out var result) ? result : defaultValue;
        }

        /// <summary>
        /// Formata valor monetário.
        /// </summary>
        public static string FormatCurrency(decimal value)
        {
            return value.ToString("C2", CultureInfo.GetCultureInfo("pt-BR"));
        }

        #endregion

        #region Date Utilities

        /// <summary>
        /// Converte string para DateTime de forma segura.
        /// </summary>
        public static DateTime SafeParseDate(string value, DateTime defaultValue = default)
        {
            if (string.IsNullOrWhiteSpace(value)) return defaultValue;

            var formats = new[]
            {
                "dd/MM/yyyy",
                "dd/MM/yyyy HH:mm:ss",
                "yyyy-MM-dd",
                "yyyy-MM-dd HH:mm:ss",
                "dd-MM-yyyy",
                "MM/dd/yyyy"
            };

            foreach (var format in formats)
            {
                if (DateTime.TryParseExact(value, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
                {
                    return result;
                }
            }

            return DateTime.TryParse(value, out var parsed) ? parsed : defaultValue;
        }

        /// <summary>
        /// Calcula a idade baseada na data de nascimento.
        /// </summary>
        public static int CalculateAge(DateTime birthDate)
        {
            var today = DateTime.Today;
            var age = today.Year - birthDate.Year;
            
            if (birthDate.Date > today.AddYears(-age))
                age--;
                
            return age;
        }

        /// <summary>
        /// Verifica se uma data está vencida.
        /// </summary>
        public static bool IsExpired(DateTime date)
        {
            return date.Date < DateTime.Today;
        }

        /// <summary>
        /// Obtém o próximo dia útil.
        /// </summary>
        public static DateTime GetNextBusinessDay(DateTime date)
        {
            var nextDay = date.AddDays(1);
            
            while (nextDay.DayOfWeek == DayOfWeek.Saturday || nextDay.DayOfWeek == DayOfWeek.Sunday)
            {
                nextDay = nextDay.AddDays(1);
            }
            
            return nextDay;
        }

        #endregion

        #region Collection Utilities

        /// <summary>
        /// Verifica se uma coleção é nula ou vazia.
        /// </summary>
        public static bool IsNullOrEmpty<T>(IEnumerable<T>? collection)
        {
            return collection == null || !collection.Any();
        }

        /// <summary>
        /// Divide uma coleção em chunks menores.
        /// </summary>
        public static IEnumerable<List<T>> ChunkBy<T>(IEnumerable<T> source, int chunkSize)
        {
            if (chunkSize <= 0) throw new ArgumentException("Chunk size deve ser maior que zero", nameof(chunkSize));

            var list = source.ToList();
            for (int i = 0; i < list.Count; i += chunkSize)
            {
                yield return list.Skip(i).Take(chunkSize).ToList();
            }
        }

        #endregion

        #region File Utilities

        /// <summary>
        /// Obtém um nome de arquivo único adicionando número se necessário.
        /// </summary>
        public static string GetUniqueFileName(string filePath)
        {
            if (!File.Exists(filePath)) return filePath;

            var directory = Path.GetDirectoryName(filePath) ?? "";
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var extension = Path.GetExtension(filePath);

            int counter = 1;
            string newFilePath;
            
            do
            {
                newFilePath = Path.Combine(directory, $"{fileName}_{counter}{extension}");
                counter++;
            } 
            while (File.Exists(newFilePath));

            return newFilePath;
        }

        /// <summary>
        /// Obtém o tamanho de um arquivo formatado.
        /// </summary>
        public static string GetFormattedFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }

        #endregion
    }

    #region Logger Adapter (Consolidado)

    /// <summary>
    /// Adapter para ILogger genérico consolidado.
    /// </summary>
    public class LoggerAdapter<T> : ILogger<T>
    {
        private readonly ILogger _logger;

        public LoggerAdapter(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => 
            _logger.BeginScope(state);

        public bool IsEnabled(LogLevel logLevel) => _logger.IsEnabled(logLevel);

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            _logger.Log(logLevel, eventId, state, exception, formatter);
        }
    }

    #endregion

    #region Communication Utilities (Consolidado do SmsHelper e NotificationHelper)

    /// <summary>
    /// Classe para funcionalidades de comunicação consolidadas.
    /// </summary>
    public static class CommunicationHelper
    {
        /// <summary>
        /// Envia um SMS de forma assíncrona (simulado).
        /// </summary>
        /// <param name="numero">Número de telefone do destinatário.</param>
        /// <param name="mensagem">Mensagem do SMS.</param>
        /// <param name="logger">Logger opcional para registrar operações.</param>
        /// <returns>Task representando a operação assíncrona.</returns>
        public static async Task EnviarSmsAsync(string numero, string mensagem, ILogger? logger = null)
        {
            try
            {
                // TODO: Implementar envio real usando serviço externo
                await Task.CompletedTask;
                logger?.LogInformation($"SMS enviado para {numero}: {mensagem}");
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, $"Erro ao enviar SMS para {numero}.");
                throw;
            }
        }

        /// <summary>
        /// Envia uma notificação push de forma assíncrona (simulado).
        /// </summary>
        /// <param name="titulo">Título da notificação.</param>
        /// <param name="mensagem">Mensagem da notificação.</param>
        /// <param name="logger">Logger opcional para registrar operações.</param>
        /// <returns>Task representando a operação assíncrona.</returns>
        public static async Task EnviarNotificacaoAsync(string titulo, string mensagem, ILogger? logger = null)
        {
            try
            {
                // TODO: Implementar envio real usando serviço externo ou plataforma nativa
                await Task.CompletedTask;
                logger?.LogInformation($"Notificação enviada: {titulo} - {mensagem}");
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Erro ao enviar notificação.");
                throw;
            }
        }

        /// <summary>
        /// Valida um número de telefone brasileiro.
        /// </summary>
        /// <param name="numero">Número de telefone para validar.</param>
        /// <returns>True se o número for válido.</returns>
        public static bool IsValidPhoneNumber(string numero)
        {
            if (string.IsNullOrWhiteSpace(numero)) return false;

            var cleanNumber = Regex.Replace(numero, @"\D", "");
            
            // Formatos válidos: 11999999999 (com DDD) ou 999999999 (sem DDD)
            return cleanNumber.Length == 11 || cleanNumber.Length == 10;
        }

        /// <summary>
        /// Formata um número de telefone no padrão brasileiro.
        /// </summary>
        /// <param name="numero">Número para formatar.</param>
        /// <returns>Número formatado.</returns>
        public static string FormatPhoneNumber(string numero)
        {
            if (string.IsNullOrWhiteSpace(numero)) return string.Empty;

            var cleanNumber = Regex.Replace(numero, @"\D", "");
            
            return cleanNumber.Length switch
            {
                11 => $"({cleanNumber.Substring(0, 2)}) {cleanNumber.Substring(2, 5)}-{cleanNumber.Substring(7, 4)}",
                10 => $"({cleanNumber.Substring(0, 2)}) {cleanNumber.Substring(2, 4)}-{cleanNumber.Substring(6, 4)}",
                _ => numero
            };
        }
    }

    #endregion
} 