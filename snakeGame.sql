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
        Tematica INT NOT NULL, -- 1 est�ndar
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
            -- Comprueba si el jugador est� en una partida en curso
            SELECT @PartidaID = P.PartidaID
            FROM JugadoresXPartida JP
            JOIN Partidas P ON JP.PartidaID = P.PartidaID
            WHERE JP.JugadorID = @ExistingJugadorID AND P.Estado = 1; -- 1: En progreso

            -- Si el jugador est� en una partida en curso, elim�nalo de la partida
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

IF NOT EXISTS (SELECT * FROM sys.objects WHERE type = 'V' AND name = 'PartidasEnEspera')
BEGIN
    EXEC('
	CREATE VIEW PartidasEnEspera AS
	SELECT
		P.PartidaId,
		P.CodigoIdentificador,
		CASE
			WHEN P.TipoJuego = 1 THEN ''Tiempo''
			WHEN P.TipoJuego = 2 THEN ''Largo''
			ELSE ''Desconocido''
		END AS [TipoJuego],
		CASE
			WHEN P.Tematica = 1 THEN ''Estandar''
			WHEN P.Tematica = 2 THEN ''Fruit mix''
		END AS Tematica,
		P.CantidadJugadores,
		COUNT(JXP.JugadorID) JugadoresConectados
	FROM Partidas P
	INNER JOIN JugadoresXPartida JXP ON JXP.PartidaId = P.PartidaID
	WHERE P.Estado = 0
	GROUP BY P.PartidaId,P.CodigoIdentificador,TipoJuego,Tematica,CantidadJugadores;
	');
END

ALTER TABLE Jugadores ADD CONSTRAINT u_nickname UNIQUE (Nickname);
ALTER TABLE Partidas ADD CONSTRAINT u_codId UNIQUE (CodigoIdentificador);


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
GO

CREATE PROCEDURE sp_UnirsePartida
    @IdentificadorPartida NVARCHAR(10),
    @Nickname NVARCHAR(50),
    @ColorSerpiente NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @JugadorID INT;
    DECLARE @PartidaID INT;

    -- Obtener el ID del jugador
    SELECT @JugadorID = JugadorID
    FROM Jugadores
    WHERE LOWER(Nickname) = LOWER(@Nickname);

    -- Obtener el ID de la partida
    SELECT @PartidaID = PartidaID
    FROM Partidas
    WHERE CodigoIdentificador = @IdentificadorPartida;

    -- Verificar si el jugador ya est� en la partida
    IF NOT EXISTS (SELECT 1 FROM JugadoresXPartida WHERE JugadorID = @JugadorID AND PartidaID = @PartidaID)
    BEGIN
        -- Insertar al jugador en la partida
        INSERT INTO JugadoresXPartida (PartidaID, JugadorID, ColorSerpiente, LargoSerpiente)
        VALUES (@PartidaID, @JugadorID, @ColorSerpiente, 1);
    END
END;
go

CREATE PROCEDURE sp_AbandonarPartida
    @IdentificadorPartida NVARCHAR(10),
    @Nickname NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @JugadorID INT;
    DECLARE @PartidaID INT;

    -- Obtener el ID del jugador
    SELECT @JugadorID = JugadorID
    FROM Jugadores
    WHERE LOWER(Nickname) = LOWER(@Nickname);

    -- Obtener el ID de la partida
    SELECT @PartidaID = PartidaID
    FROM Partidas
    WHERE CodigoIdentificador = @IdentificadorPartida;

    -- Eliminar al jugador de la partida
    DELETE FROM JugadoresXPartida
    WHERE JugadorID = @JugadorID AND PartidaID = @PartidaID;
END;

SELECT * FROM Partidas WHERE CodigoIdentificador = '3342ad53-b';
select * from JugadoresXPartida JXP
INNER JOIN Partidas P ON P.PartidaID = JXP.PartidaID
INNER JOIN Jugadores J ON J.JugadorID = JXP.JugadorID
WHERE P.CodigoIdentificador = 'd7b5ce9b-7'


IF NOT EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_InsertarPartida')
BEGIN
    EXEC('
    CREATE PROCEDURE sp_InsertarPartida
        @TipoJuego INT,
        @Extension INT,
        @Tematica INT,
        @CodigoIdentificador NVARCHAR(10),
        @JugadorID INT,
		@Cantidad INT
    AS
    BEGIN
        SET NOCOUNT ON;
        DECLARE @PartidaID INT;

        -- Verifica si el CodigoIdentificador ya existe
        IF NOT EXISTS (SELECT 1 FROM Partidas WHERE CodigoIdentificador = @CodigoIdentificador)
        BEGIN
            -- Inserta la partida
            INSERT INTO Partidas (CodigoIdentificador, TipoJuego, Tematica, TiempoRestante, LargoObjetivo, Estado, CantidadJugadores)
            VALUES (@CodigoIdentificador, @TipoJuego, @Tematica, 
                    CASE WHEN @TipoJuego = 1 THEN @Extension ELSE NULL END, -- TiempoRestante
                    CASE WHEN @TipoJuego = 2 THEN @Extension ELSE NULL END, -- LargoObjetivo
                    0,@Cantidad); -- Inicializa el Estado con 0

            -- Obtiene el ID de la partida reci�n insertada
            SET @PartidaID = SCOPE_IDENTITY();

            -- Inserta el jugador en la partida
            INSERT INTO JugadoresXPartida (PartidaID, JugadorID, ColorSerpiente, LargoSerpiente)
            VALUES (@PartidaID, @JugadorID, ''ND'', 1);
        END
        ELSE
        BEGIN
            PRINT ''El CodigoIdentificador ya existe. No se ha insertado la partida.'';
        END
    END;
    ');
END

-- Verifica si el procedimiento almacenado existe antes de eliminarlo
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_InsertarPartida')
BEGIN
    -- Elimina el procedimiento almacenado
    DROP PROCEDURE sp_InsertarPartida;
    PRINT 'El procedimiento almacenado ha sido eliminado.';
END
ELSE
BEGIN
    PRINT 'El procedimiento almacenado no existe.';
END


DECLARE @CodigoIdentificador NVARCHAR(10) = 'AHH563';
DECLARE @Tipo INT = 2;
DECLARE @Extension INT = 50;
DECLARE @Tematica INT = 1;
DECLARE @IDJugador INT = 24;

EXEC sp_InsertarPartida
    @TipoJuego = @Tipo,
    @Extension = @Extension,
    @Tematica = @Tematica,
    @CodigoIdentificador = @CodigoIdentificador,
	@JugadorID = @IDJugador,
	@Cantidad = 2;

select *from JugadoresXPartida
select *from Partidas


Delete from JugadoresXPartida
where ColorSerpiente = 'ND';

Delete from partidas
where Estado = 0;


select *from Jugadores Where Nickname = 'Andy' 


select * from Partidas where CodigoIdentificador = 'ABCY123'
