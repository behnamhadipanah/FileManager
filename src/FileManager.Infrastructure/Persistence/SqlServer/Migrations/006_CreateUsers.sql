IF NOT EXISTS (
    SELECT 1
    FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE s.name = '{{Schema}}' AND t.name = 'Users')
BEGIN
    CREATE TABLE [{{Schema}}].[Users]
    (
        Id           BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
        FirstName    NVARCHAR(100)        NOT NULL,
        LastName     NVARCHAR(100)        NOT NULL,
        Email        NVARCHAR(256)        NOT NULL,
        PasswordHash NVARCHAR(512)        NOT NULL,
        IsActive     BIT                  NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT (1),
        CreationTime DATETIME2            NOT NULL
    );

    CREATE UNIQUE INDEX UX_Users_Email ON [{{Schema}}].[Users] (Email);
END
