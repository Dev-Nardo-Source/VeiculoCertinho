-- Script para criar a tabela VeiculoDetalhes no banco SQLite
CREATE TABLE IF NOT EXISTS VeiculoDetalhes (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    VeiculoId INTEGER NOT NULL,
    Descricao TEXT,
    Valor TEXT,
    DataRegistro DATETIME,
    FOREIGN KEY (VeiculoId) REFERENCES Veiculo(Id)
);
