-- Создаем базу заново
CREATE DATABASE FootballDb;
GO
USE FootballDb;
GO

-- 1. Таблица команд
CREATE TABLE Teams (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL
);
GO

-- 2. Таблица игроков
CREATE TABLE Players (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Goals INT NOT NULL DEFAULT 0,
    TeamId INT NOT NULL FOREIGN KEY REFERENCES Teams(Id) ON DELETE CASCADE
);
GO

-- 3. Таблица матчей
CREATE TABLE Matches (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    HomeTeamId INT NOT NULL FOREIGN KEY REFERENCES Teams(Id),
    AwayTeamId INT NOT NULL FOREIGN KEY REFERENCES Teams(Id),
    HomeGoals INT NOT NULL DEFAULT 0,
    AwayGoals INT NOT NULL DEFAULT 0,
    MatchDate DATETIME NOT NULL
);
GO

-- 4. Хранимая процедура для генерации матчей 
CREATE OR ALTER PROCEDURE sp_GenerateMatches
AS
BEGIN
    DELETE FROM Matches;

    DECLARE @Team1 INT, @Team2 INT, @Goals1 INT, @Goals2 INT;
    DECLARE @i INT = 1;

    -- Генерим 20 случайных матчей
    WHILE @i <= 20
    BEGIN
        -- случайные ID команд
        SET @Team1 = (SELECT TOP 1 Id FROM Teams ORDER BY NEWID());
        SET @Team2 = (SELECT TOP 1 Id FROM Teams WHERE Id != @Team1 ORDER BY NEWID());
        
        -- Случайные голы
        SET @Goals1 = ABS(CHECKSUM(NEWID())) % 6;
        SET @Goals2 = ABS(CHECKSUM(NEWID())) % 6;

        INSERT INTO Matches (HomeTeamId, AwayTeamId, HomeGoals, AwayGoals, MatchDate)
        VALUES (@Team1, @Team2, @Goals1, @Goals2, GETDATE());

        SET @i = @i + 1;
    END
END;
GO