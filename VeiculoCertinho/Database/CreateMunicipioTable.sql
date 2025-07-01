-- Tabela para armazenar os municípios do Brasil
CREATE TABLE IF NOT EXISTS Municipio (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Nome TEXT NOT NULL,
    UfId INTEGER NOT NULL,
    FOREIGN KEY (UfId) REFERENCES UF(Id)
); 