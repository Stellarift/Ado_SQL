CREATE DATABASE MailingDapperDb;
GO
USE MailingDapperDb;
GO

-- 1. Покупатели
CREATE TABLE Customers (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(200) NOT NULL,
    BirthDate DATE NOT NULL,
    Gender NVARCHAR(10) NOT NULL, -- 'М' или 'Ж'
    Email NVARCHAR(200) NOT NULL,
    Country NVARCHAR(100) NOT NULL,
    City NVARCHAR(100) NOT NULL
);

-- 2. Разделы (интересы)
CREATE TABLE Sections (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL
);

-- 3. Связь покупателей и разделов (многие ко многим)
CREATE TABLE CustomerSections (
    CustomerId INT NOT NULL FOREIGN KEY REFERENCES Customers(Id),
    SectionId INT NOT NULL FOREIGN KEY REFERENCES Sections(Id),
    PRIMARY KEY (CustomerId, SectionId)
);

-- 4. Акционные товары
CREATE TABLE PromoProducts (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    SectionId INT NOT NULL FOREIGN KEY REFERENCES Sections(Id),
    Country NVARCHAR(100) NOT NULL, -- Акция привязана к стране
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL
);
GO

-- ТЕСТОВЫЕ ДАННЫЕ
INSERT INTO Sections (Name) VALUES 
(N'Мобильные телефоны'), (N'Ноутбуки'), (N'Кухонная техника'), (N'Телевизоры');

INSERT INTO Customers (FullName, BirthDate, Gender, Email, Country, City) VALUES
(N'Иванов Иван', '1990-05-15', N'М', 'ivan@mail.ru', N'Россия', N'Москва'),
(N'Петрова Анна', '1995-08-20', N'Ж', 'anna@mail.ru', N'Россия', N'Санкт-Петербург'),
(N'Сидоров Олег', '1988-12-01', N'М', 'oleg@mail.ru', N'Россия', N'Москва'),
(N'Смит Джон', '1992-03-10', N'М', 'john@mail.com', N'США', N'Нью-Йорк'),
(N'Гарсия Мария', '1998-07-25', N'Ж', 'maria@mail.com', N'Испания', N'Мадрид');

INSERT INTO CustomerSections (CustomerId, SectionId) VALUES
(1, 1), (1, 2), -- Иван интересуется телефонами и ноутбуками
(2, 3),         -- Анна интересуется кухонной техникой
(3, 1), (3, 4), -- Олег интересуется телефонами и ТВ
(4, 2),         -- Джон интересуется ноутбуками
(5, 3);         -- Мария интересуется кухонной техникой

INSERT INTO PromoProducts (Name, SectionId, Country, StartDate, EndDate) VALUES
(N'Скидка на iPhone 15', 1, N'Россия', '2026-10-01', '2026-11-01'),
(N'Скидка на MacBook', 2, N'Россия', '2026-10-15', '2026-11-15'),
(N'Блендер в подарок', 3, N'Россия', '2026-10-10', '2026-10-30'),
(N'TV Samsung -20%', 4, N'Россия', '2026-10-01', '2026-10-31'),
(N'Black Friday Phones', 1, N'США', '2026-11-20', '2026-11-30'),
(N'Black Friday Laptops', 2, N'США', '2026-11-20', '2026-11-30');
GO