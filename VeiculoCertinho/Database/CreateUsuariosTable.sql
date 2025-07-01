-- Script para criar a tabela Usuarios no banco SQLite
CREATE TABLE IF NOT EXISTS Usuarios (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Nome TEXT NOT NULL,
    Email TEXT NOT NULL,
    SenhaHash TEXT NOT NULL,
    DoisFatoresAtivado BOOLEAN NOT NULL DEFAULT 0,
    Perfil TEXT NOT NULL,
    DataCriacao DATETIME NOT NULL,
    UltimoLogin DATETIME
);
