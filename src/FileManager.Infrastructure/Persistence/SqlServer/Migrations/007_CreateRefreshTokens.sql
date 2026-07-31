IF NOT EXISTS (
    SELECT 1
    FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE s.name = '{{Schema}}' AND t.name = 'RefreshTokens')
BEGIN
    CREATE TABLE [{{Schema}}].[RefreshTokens]
    (
        Id              UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_RefreshTokens PRIMARY KEY,
        UserId          BIGINT           NOT NULL,
        Token           NVARCHAR(512)    NOT NULL,
        Expires         DATETIME2        NOT NULL,
        Created         DATETIME2        NOT NULL,
        CreatedByIp     NVARCHAR(64)     NULL,
        Revoked         DATETIME2        NULL,
        RevokedByIp     NVARCHAR(64)     NULL,
        ReplacedByToken NVARCHAR(512)    NULL,
        CONSTRAINT FK_RefreshTokens_Users FOREIGN KEY (UserId) REFERENCES [{{Schema}}].[Users] (Id)
    );

    CREATE UNIQUE INDEX UX_RefreshTokens_Token ON [{{Schema}}].[RefreshTokens] (Token);
    CREATE INDEX IX_RefreshTokens_UserId ON [{{Schema}}].[RefreshTokens] (UserId);
END
