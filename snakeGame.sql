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

GO

-- Verifica si el procedimiento almacenado ya existe
IF NOT EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_InsertarJugador')
BEGIN
    EXEC('
    CREATE PROCEDURE sp_InsertarJugador
        @Nickname NVARCHAR(50)
    AS
    BEGIN
        SET NOCOUNT ON;
        DECLARE @ExistingJugadorID INT;
        DECLARE @PartidaID INT;

        -- Comprueba si el jugador ya existe
        SELECT @ExistingJugadorID = JugadorID
        FROM Jugadores
        WHERE LOWER(Nickname) = LOWER(@Nickname);

        -- Si el jugador no existe, lo inserta
        IF @ExistingJugadorID IS NULL
        BEGIN
            INSERT INTO Jugadores (Nickname)
            VALUES (LOWER(@Nickname));
        END
        ELSE
        BEGIN
            -- Comprueba si el jugador está en una partida en curso
            SELECT @PartidaID = P.PartidaID
            FROM JugadoresXPartida JP
            JOIN Partidas P ON JP.PartidaID = P.PartidaID
            WHERE JP.JugadorID = @ExistingJugadorID AND P.Estado = 1; -- 1: En progreso

            -- Si el jugador está en una partida en curso, elimínalo de la partida
            IF @PartidaID IS NOT NULL
            BEGIN
                DELETE FROM JugadoresXPartida
                WHERE PartidaID = @PartidaID AND JugadorID = @ExistingJugadorID;
            END
        END
    END;');
END

IF NOT EXISTS (SELECT * FROM sys.objects WHERE type = 'V' AND name = 'Ranking')
BEGIN
    EXEC('
    CREATE VIEW Ranking AS
    SELECT
        J.Nickname AS Ganador,
        JXP.LargoSerpiente AS Largo,
        P.Tematica AS Tematica,
        CASE
            WHEN P.TipoJuego = 1 THEN ''Tiempo''
            WHEN P.TipoJuego = 2 THEN ''Largo''
            ELSE ''Desconocido''
        END AS [Tipo],
        P.CodigoIdentificador AS IdentificadorPartida
    FROM Ganadores G
    INNER JOIN JugadoresXPartida JXP ON G.JugadorXPartidaID = JXP.JugadorXPartidaID
    INNER JOIN Jugadores J ON JXP.JugadorID = J.JugadorID
    INNER JOIN Partidas P ON G.PartidaID = P.PartidaID;
    ');
END

ALTER TABLE Jugadores ADD CONSTRAINT u_nickname UNIQUE (Nickname);


select * from Jugadores;
SELECT * FROM ranking;