create database MyORM_DB

use MyORM_DB

CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TelegramId VARCHAR(100) NOT NULL UNIQUE,
    FullName VARCHAR(255) NOT NULL,
    NickName VARCHAR(255),
    Login VARCHAR(100) NOT NULL UNIQUE,
    Password VARCHAR(255) NOT NULL,
    Role VARCHAR(50) NOT NULL
);

Truncate TABLE Users;

Select * from Users;

Drop table Users;