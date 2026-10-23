CREATE DATABASE MailingDb;
GO
USE MailingDb;
GO

-- категории
CREATE TABLE Sections (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL
);

--  Покупатели
CREATE TABLE Customers (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Gender NVARCHAR(10) NOT NULL, -- "Мужской" или "Женский"
    Age INT NOT NULL,
    City NVARCHAR(100) NOT NULL,
    Country NVARCHAR(100) NOT NULL,
    SectionId INT NOT NULL FOREIGN KEY REFERENCES Sections(Id)
);

-- 3. Товары
CREATE TABLE Products (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    SectionId INT NOT NULL FOREIGN KEY REFERENCES Sections(Id),
    IsPromotional BIT NOT NULL DEFAULT 0, -- 1 - акционный, 0 - обычный
    PromoEndDate DATETIME NULL
);

-- Архив акций 
CREATE TABLE PromoArchive (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    ProductId INT NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    ArchivedAt DATETIME NOT NULL DEFAULT GETDATE()
);
GO

-- Заполняем тестовыми данными
INSERT INTO Sections (Name) VALUES (N'Спорт'), (N'Кулинария'), (N'Технологии'), (N'Мода'), (N'Авто');
GO

INSERT INTO Customers (Name, Gender, Age, City, Country, SectionId) VALUES
(N'Иван', N'Мужской', 25, N'Москва', N'Россия', 1),
(N'Анна', N'Женский', 30, N'Москва', N'Россия', 2),
(N'Петр', N'Мужской', 45, N'Санкт-Петербург', N'Россия', 1),
(N'Мария', N'Женский', 22, N'Минск', N'Беларусь', 4),
(N'Олег', N'Мужской', 35, N'Киев', N'Украина', 3),
(N'Елена', N'Женский', 28, N'Москва', N'Россия', 2),
(N'Сергей', N'Мужской', 50, N'Алматы', N'Казахстан', 5);
GO

-- Даты: DATEADD(DAY, 3, GETDATE()) - через 3 дня, DATEADD(DAY, -5, GETDATE()) - просрочено
INSERT INTO Products (Name, SectionId, IsPromotional, PromoEndDate) VALUES
(N'Гантели', 1, 1, DATEADD(DAY, 3, GETDATE())),  -- Заканчивается через 3 дня
(N'Кроссовки', 1, 1, DATEADD(DAY, -5, GETDATE())), -- Просрочено
(N'Сковорода', 2, 1, DATEADD(DAY, 10, GETDATE())), -- Акционный
(N'Ноутбук', 3, 0, NULL),                        -- Обычный
(N'Платье', 4, 1, DATEADD(DAY, 3, GETDATE())),   -- Заканчивается через 3 дня
(N'Шины', 5, 0, NULL);                           -- Обычный
GO