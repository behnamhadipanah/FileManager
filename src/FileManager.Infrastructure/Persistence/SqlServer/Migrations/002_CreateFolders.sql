IF NOT EXISTS (
    SELECT 1
    FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE s.name = '{{Schema}}' AND t.name = 'Folders')
BEGIN
    CREATE TABLE [{{Schema}}].[Folders]
    (
        Id                   BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Folders PRIMARY KEY,
        BusinessId           UNIQUEIDENTIFIER      NOT NULL,
        ApplicationId        BIGINT                NOT NULL CONSTRAINT FK_Folders_Applications REFERENCES [{{Schema}}].[Applications] (Id),
        ParentFolderId       BIGINT                NULL CONSTRAINT FK_Folders_ParentFolder REFERENCES [{{Schema}}].[Folders] (Id),
        Name                 NVARCHAR(255)         NOT NULL,
        IsDeleted            BIT                   NOT NULL CONSTRAINT DF_Folders_IsDeleted DEFAULT (0),
        DeletionTime         DATETIME2             NULL,
        CreationTime         DATETIME2             NOT NULL,
        CreatorId            BIGINT                NOT NULL CONSTRAINT DF_Folders_CreatorId DEFAULT (0),
        LastModificationTime DATETIME2             NULL,
        LastModifierId       BIGINT                NULL
    );

    CREATE UNIQUE INDEX UX_Folders_BusinessId ON [{{Schema}}].[Folders] (BusinessId);
    CREATE INDEX IX_Folders_Application_Parent ON [{{Schema}}].[Folders] (ApplicationId, ParentFolderId) INCLUDE (Name, IsDeleted);
END
