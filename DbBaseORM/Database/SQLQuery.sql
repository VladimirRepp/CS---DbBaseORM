Create database DbBaseORM_DB

Use DbBaseORM_DB

CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TelegramId VARCHAR(100) NOT NULL UNIQUE,
    FullName VARCHAR(255) NOT NULL,
    NickName VARCHAR(255),
    Password VARCHAR(255) NOT NULL,
    Role VARCHAR(50) NOT NULL -- Example Of Values: Student, Teacher, Admin
);

Select * from Users;

Truncate table Users;
--Drop table Users;
