IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Applications')
BEGIN
    CREATE TABLE Applications
    (
        Id                   BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Applications PRIMARY KEY,
        BusinessId           UNIQUEIDENTIFIER      NOT NULL,
        ApplicationName      NVARCHAR(200)         NOT NULL,
        Token                NVARCHAR(256)         NOT NULL,
        MinImageSize         BIGINT                NOT NULL,
        MaxImageSize         BIGINT                NOT NULL,
        MinVideoSize         BIGINT                NOT NULL,
        MaxVideoSize         BIGINT                NOT NULL,
        MinDocumentSize      BIGINT                NOT NULL,
        MaxDocumentSize      BIGINT                NOT NULL,
        IsActive             BIT                   NOT NULL CONSTRAINT DF_Applications_IsActive DEFAULT (1),
        CreationTime         DATETIME2             NOT NULL,
        CreatorId            BIGINT                NOT NULL CONSTRAINT DF_Applications_CreatorId DEFAULT (0),
        LastModificationTime DATETIME2             NULL,
        LastModifierId       BIGINT                NULL
    );

    CREATE UNIQUE INDEX UX_Applications_BusinessId ON Applications (BusinessId);
    CREATE UNIQUE INDEX UX_Applications_ApplicationName ON Applications (ApplicationName);
    CREATE UNIQUE INDEX UX_Applications_Token ON Applications (Token);
END
