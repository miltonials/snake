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


-- Tabla de Jugadores
INSERT INTO Jugadores (Nickname) VALUES ('player1'), ('player2'), ('player3'), ('player4'), ('player5'), ('player6'), ('player7'), ('player8'), ('player9'), ('player10');

-- Tabla de Partidas
INSERT INTO Partidas (CodigoIdentificador, TipoJuego, Tematica, TiempoRestante, LargoObjetivo, Estado) VALUES 
('ABC123', 1, 1, 300, NULL, 2),
('DEF456', 2, 1, NULL, 10, 0),
('GHI789', 1, 1, 600, NULL, 1),
('JKL012', 2, 1, NULL, 15, 2),
('MNO345', 1, 1, 450, NULL, 0),
('PQR678', 2, 1, NULL, 12, 2),
('STU901', 1, 1, 240, NULL, 1),
('VWX234', 2, 1, NULL, 8, 2),
('YZA567', 1, 1, 180, NULL, 0),
('BCD890', 2, 1, NULL, 20, 1);

-- Tabla de JugadoresXPartida
INSERT INTO JugadoresXPartida (PartidaID, JugadorID, ColorSerpiente, LargoSerpiente) VALUES
(1, 1, 'Blue', 1),
(1, 2, 'Red', 1),
(1, 3, 'Green', 1),
(2, 4, 'Yellow', 1),
(2, 5, 'Purple', 1),
(2, 6, 'Orange', 1),
(3, 7, 'Pink', 1),
(3, 8, 'Cyan', 1),
(3, 9, 'Brown', 1),
(4, 10, 'White', 1);

-- Tabla de Ganadores
INSERT INTO Ganadores (PartidaID, JugadorXPartidaID) VALUES
(1, 1),
(2, 5),
(3, 9),
(4, 6),
(5, 3),
(6, 7),
(7, 8),
(8, 2),
(9, 4),
(10, 10);

IF NOT EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_InsertarPartida')
BEGIN
EXEC('
    CREATE PROCEDURE sp_InsertarPartida
        @CodigoIdentificador NVARCHAR(10),
        @Tipo INT,
        @Extension INT,
        @Tematica INT
    AS
    BEGIN
        SET NOCOUNT ON;

        -- Verifica que el Tipo sea válido
        IF @Tipo NOT IN (1, 2)
        BEGIN
            THROW 50000, "El Tipo proporcionado no es válido.", 1;
            RETURN;
        END

        -- Verifica que la Extensión sea válida (puedes extender la validación según sea necesario)
        IF @Extension IS NULL
        BEGIN
            THROW 50000, 'La Extensión es obligatoria.', 1;
            RETURN;
        END

        -- Verifica que la Temática sea válida (puedes extender la validación según sea necesario)
        IF @Tematica IS NULL
        BEGIN
            THROW 50000, 'La Temática es obligatoria.', 1;
            RETURN;
        END

        -- Inserta la nueva partida
        INSERT INTO Partidas (CodigoIdentificador, TipoJuego, Tematica, TiempoRestante, LargoObjetivo, Estado)
        VALUES (@CodigoIdentificador, @Tipo, @Tematica, 
                CASE WHEN @Tipo = 1 THEN @Extension ELSE NULL END, -- TiempoRestante
                CASE WHEN @Tipo = 2 THEN @Extension ELSE NULL END, -- LargoObjetivo
                0); -- Inicializa el Estado con 0
    END;
	');
END

-- Declarar variables de parámetros
DECLARE @CodigoIdentificador NVARCHAR(10) = 'ABC123';
DECLARE @Tipo INT = 1;
DECLARE @Extension INT = 30;
DECLARE @Tematica INT = 5;
DECLARE @Cantidad INT = 2;

-- Ejecutar el procedimiento almacenado
EXEC sp_InsertarPartida
    @CodigoIdentificador = @CodigoIdentificador,
    @Tipo = @Tipo,
    @Extension = @Extension,
    @Tematica = @Tematica,
    @Cantidad = @Cantidad;

SELECT * FROM Partidas;
