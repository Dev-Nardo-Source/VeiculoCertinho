using Microsoft.Data.Sqlite;
using System.IO;
using System.Reflection;

namespace VeiculoCertinho.Database
{
    public static class DatabaseConfig
    {
        private const string DatabaseName = "veiculocertinho.db";
        public static string DatabasePath => Path.Combine(FileSystem.AppDataDirectory, DatabaseName);

        public static void InitializeDatabase()
        {
            using var connection = new SqliteConnection($"Data Source={DatabasePath}");
            connection.Open();

            // Criar tabelas usando recursos incorporados
            ExecuteEmbeddedScript(connection, "VeiculoCertinho.Database.CreateUsuariosTable.sql");
            ExecuteEmbeddedScript(connection, "VeiculoCertinho.Database.CreateUfTable.sql");
            ExecuteEmbeddedScript(connection, "VeiculoCertinho.Database.CreateMunicipioTable.sql");
            
            // Migrar tabela Veiculo se necessário
            MigrateVeiculoTable(connection);
            
            ExecuteEmbeddedScript(connection, "VeiculoCertinho.Database.CreateVeiculoDetalhesTable.sql");
        }

        private static void MigrateVeiculoTable(SqliteConnection connection)
        {
            try
            {
                // Verificar se a tabela Veiculo existe e qual é sua estrutura
                var checkCommand = connection.CreateCommand();
                checkCommand.CommandText = "PRAGMA table_info(Veiculo);";
                
                bool hasUfOrigemId = false;
                bool hasUfAtualId = false;
                
                using (var reader = checkCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var columnName = reader.GetString(1); // column name
                        if (columnName == "UfOrigemId") hasUfOrigemId = true;
                        if (columnName == "UfAtualId") hasUfAtualId = true;
                    }
                }

                if (!hasUfOrigemId || !hasUfAtualId)
                {
                    // Precisamos migrar a tabela
                    PerformVeiculoMigration(connection);
                }
            }
            catch (Exception ex)
            {
                // Se a tabela não existe, criar do zero
                ExecuteEmbeddedScript(connection, "VeiculoCertinho.Database.CreateVeiculoTable.sql");
            }
        }

        private static void PerformVeiculoMigration(SqliteConnection connection)
        {
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    // 1. Renomear tabela existente
                    var renameCmd = connection.CreateCommand();
                    renameCmd.CommandText = "ALTER TABLE Veiculo RENAME TO Veiculo_old;";
                    renameCmd.ExecuteNonQuery();

                    // 2. Criar nova tabela com estrutura atualizada
                    ExecuteEmbeddedScript(connection, "VeiculoCertinho.Database.CreateVeiculoTable.sql");

                    // 3. Migrar dados da tabela antiga para a nova
                    var migrateCmd = connection.CreateCommand();
                    migrateCmd.CommandText = @"
                        INSERT INTO Veiculo (
                            Id, Chassi, Placa, UsaGasolina, UsaEtanol, UsaGNV, UsaRecargaEletrica, 
                            UsaDiesel, UsaHidrogenio, Marca, Modelo, AnoFabricacao, AnoModelo, 
                            Cor, Motor, Cilindrada, Potencia, Importado, UfOrigemId, UfAtualId, 
                            MunicipioOrigem, MunicipioAtual, Segmento, EspecieVeiculo, Passageiros, 
                            Observacoes, Personalizacao
                        )
                        SELECT 
                            v.Id, v.Chassi, v.Placa, v.UsaGasolina, v.UsaEtanol, v.UsaGNV, 
                            v.UsaRecargaEletrica, v.UsaDiesel, v.UsaHidrogenio, v.Marca, v.Modelo, 
                            v.AnoFabricacao, v.AnoModelo, v.Cor, v.Motor, v.Cilindrada, v.Potencia, 
                            v.Importado, 
                            COALESCE(uf_origem.Id, 35) as UfOrigemId, -- Default para SP (35)
                            COALESCE(uf_atual.Id, 35) as UfAtualId,   -- Default para SP (35)
                            v.MunicipioOrigem, v.MunicipioAtual, v.Segmento, v.EspecieVeiculo, 
                            v.Passageiros, v.Observacoes, v.Personalizacao
                        FROM Veiculo_old v
                        LEFT JOIN UF uf_origem ON v.UfOrigem = uf_origem.Sigla
                        LEFT JOIN UF uf_atual ON v.UfAtual = uf_atual.Sigla;
                    ";
                    migrateCmd.ExecuteNonQuery();

                    // 4. Remover tabela antiga
                    var dropCmd = connection.CreateCommand();
                    dropCmd.CommandText = "DROP TABLE Veiculo_old;";
                    dropCmd.ExecuteNonQuery();

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public static void EnsureUfTableExists()
        {
            using var connection = new SqliteConnection($"Data Source={DatabasePath}");
            connection.Open();

            // Verificar se a tabela UF existe
            var command = connection.CreateCommand();
            command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='UF';";
            var result = command.ExecuteScalar();

            if (result == null)
            {
                // Tabela não existe, criar
                ExecuteEmbeddedScript(connection, "VeiculoCertinho.Database.CreateUfTable.sql");
            }
        }

        private static void ExecuteEmbeddedScript(SqliteConnection connection, string resourceName)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream == null)
                {
                    throw new Exception($"Recurso incorporado não encontrado: {resourceName}");
                }
                
                using var reader = new StreamReader(stream);
                var script = reader.ReadToEnd();
                
                using var command = connection.CreateCommand();
                command.CommandText = script;
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao executar script {resourceName}: {ex.Message}", ex);
            }
        }

        private static void ExecuteScript(SqliteConnection connection, string scriptPath)
        {
            var script = File.ReadAllText(scriptPath);
            using var command = connection.CreateCommand();
            command.CommandText = script;
            command.ExecuteNonQuery();
        }

        public static SqliteConnection GetConnection()
        {
            var connection = new SqliteConnection($"Data Source={DatabasePath}");
            connection.Open();
            return connection;
        }
    }
} 