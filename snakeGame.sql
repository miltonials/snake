IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'SnakeGame')
BEGIN
    CREATE DATABASE SnakeGame;
END

-- Usar la base de datos
USE SnakeGame;

-- Tabla de Jugadores
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'Jugadores') AND type in (N'U'))
BEGIN
    CREATE TABLE Jugadores (
        JugadorID INT PRIMARY KEY IDENTITY(1,1),
        Nickname NVARCHAR(50) NOT NULL
    );
END

-- Tabla de Partidas
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'Partidas') AND type in (N'U'))
BEGIN
    CREATE TABLE Partidas (
        PartidaID INT PRIMARY KEY IDENTITY(1,1),
        CodigoIdentificador NVARCHAR(10) NOT NULL,
        TipoJuego INT NOT NULL, -- 1 para Tiempo, 2 para Largo
        Tematica INT NOT NULL, -- 1 estándar
        TiempoRestante INT, -- Solo si es TipoJuego 1
        LargoObjetivo INT, -- Solo si es TipoJuego 2
        Estado INT DEFAULT 0, -- 0: En espera, 1: En progreso, 2: Terminada
    );
END

-- Tabla de JugadoresXPartida
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'JugadoresXPartida') AND type in (N'U'))
BEGIN
    CREATE TABLE JugadoresXPartida (
        JugadorXPartidaID INT PRIMARY KEY IDENTITY(1,1),
        PartidaID INT,
        JugadorID INT,
        ColorSerpiente NVARCHAR(20) NOT NULL,
        LargoSerpiente INT DEFAULT 1,
        
        FOREIGN KEY (PartidaID) REFERENCES Partidas(PartidaID),
        FOREIGN KEY (JugadorID) REFERENCES Jugadores(JugadorID)
    );
END

-- Tabla de Ganadores
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'Ganadores') AND type in (N'U'))
BEGIN
    CREATE TABLE Ganadores (
        GanadorID INT PRIMARY KEY IDENTITY(1,1),
        PartidaID INT,
        JugadorXPartidaID INT,
        
        FOREIGN KEY (PartidaID) REFERENCES Partidas(PartidaID),
        FOREIGN KEY (JugadorXPartidaID) REFERENCES JugadoresXPartida(JugadorXPartidaID)
    );
END
