CREATE DATABASE StudentGrades;
GO

USE StudentGrades;
GO

CREATE TABLE Students (
    Id        INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    FullName  NVARCHAR(200) NOT NULL,
    GroupName NVARCHAR(50) NOT NULL
);
GO

CREATE TABLE Subjects (
    Id   INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL
);
GO

CREATE TABLE Grades (
    Id        INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    StudentId INT NOT NULL,
    SubjectId INT NOT NULL,
    Grade     INT NOT NULL,
    GradeDate DATE NOT NULL,
    FOREIGN KEY (StudentId) REFERENCES Students (Id),
    FOREIGN KEY (SubjectId) REFERENCES Subjects (Id),
    CONSTRAINT CK_Grades_Grade CHECK (Grade BETWEEN 1 AND 5)
);
GO

INSERT INTO Students (FullName, GroupName) VALUES
(N'Иванов Иван',    N'P101'),
(N'Петров Пётр',    N'P101'),
(N'Сидорова Анна',  N'P102'),
(N'Кузнецов Олег',  N'P102');
GO

INSERT INTO Subjects (Name) VALUES
(N'Математика'),
(N'Физика'),
(N'Программирование');
GO

INSERT INTO Grades (StudentId, SubjectId, Grade, GradeDate) VALUES
(1, 1, 5, '2025-01-15'),
(1, 2, 4, '2025-02-20'),
(2, 1, 3, '2025-01-18'),
(3, 3, 5, '2025-03-01'),
(4, 2, 4, '2025-03-10');
GO