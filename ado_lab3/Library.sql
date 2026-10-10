CREATE DATABASE Library;
GO

USE Library
GO

CREATE TABLE Books (
    Id     INT PRIMARY KEY IDENTITY(1,1),
    Title  NVARCHAR(100),
    Author NVARCHAR(50),
    Price  INT,
    Pages  INT
);
GO

INSERT INTO Books (Title, Author, Price, Pages) VALUES
('I Robot',                'Isaac Asimov',    178, 358),
('The Door into Summer',   'Robert Heinlein', 220, 410),
('The Robots of Dawn',     'Isaac Asimov',    250, 480),
('The Goblin Reservation', 'Clifford Simak',  230, 450);
GO

CREATE TABLE Authors (
    Id        INT PRIMARY KEY,
    FirstName NVARCHAR(50),
    LastName  NVARCHAR(50)
);
GO

INSERT INTO Authors VALUES
(1,    'Isaac',    'Asimov'),
(2,    'Ray',      'Bradbury'),
(3,    'Clifford', 'Simak'),
(1002, 'Roger',    'Zelazny');
GO