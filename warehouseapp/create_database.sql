CREATE DATABASE Warehouse;
GO
USE Warehouse;
GO

CREATE TABLE Suppliers (
    Id   INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL
);
CREATE TABLE ProductTypes (
    Id   INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL
);
CREATE TABLE Products (
    Id           INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Name         NVARCHAR(200) NOT NULL,
    TypeId       INT NOT NULL,
    SupplierId   INT NOT NULL,
    Quantity     INT NOT NULL,
    CostPrice    MONEY NOT NULL,
    DeliveryDate DATE NOT NULL,
    FOREIGN KEY (TypeId) REFERENCES ProductTypes (Id),
    FOREIGN KEY (SupplierId) REFERENCES Suppliers (Id)
);
GO

INSERT INTO Suppliers (Name) VALUES (N'ООО Ромашка'), (N'ИП Иванов');
INSERT INTO ProductTypes (Name) VALUES (N'Электроника'), (N'Одежда');
INSERT INTO Products (Name, TypeId, SupplierId, Quantity, CostPrice, DeliveryDate) VALUES
(N'Телевизор', 1, 1, 15, 30000, '2025-01-15'),
(N'Ноутбук',   1, 2, 8,  55000, '2025-03-20'),
(N'Футболка',  2, 1, 100, 500,  '2025-02-10');
GO