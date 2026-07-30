IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Folders')
BEGIN
    CREATE TABLE Folders
    (
        Id                   BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Folders PRIMARY KEY,
        BusinessId           UNIQUEIDENTIFIER      NOT NULL,
        ApplicationId        BIGINT                NOT NULL CONSTRAINT FK_Folders_Applications REFERENCES Applications (Id),
        ParentFolderId       BIGINT                NULL CONSTRAINT FK_Folders_ParentFolder REFERENCES Folders (Id),
        Name                 NVARCHAR(255)         NOT NULL,
        IsDeleted            BIT                   NOT NULL CONSTRAINT DF_Folders_IsDeleted DEFAULT (0),
        DeletionTime         DATETIME2             NULL,
        CreationTime         DATETIME2             NOT NULL,
        CreatorId            BIGINT                NOT NULL CONSTRAINT DF_Folders_CreatorId DEFAULT (0),
        LastModificationTime DATETIME2             NULL,
        LastModifierId       BIGINT                NULL
    );

    CREATE UNIQUE INDEX UX_Folders_BusinessId ON Folders (BusinessId);
    CREATE INDEX IX_Folders_Application_Parent ON Folders (ApplicationId, ParentFolderId) INCLUDE (Name, IsDeleted);
END
