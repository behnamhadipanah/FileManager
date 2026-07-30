IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'StorageFiles')
BEGIN
    CREATE TABLE StorageFiles
    (
        Id                   BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_StorageFiles PRIMARY KEY,
        BusinessId           UNIQUEIDENTIFIER      NOT NULL,
        ApplicationId        BIGINT                NOT NULL CONSTRAINT FK_StorageFiles_Applications REFERENCES Applications (Id),
        ParentFolderId       BIGINT                NULL CONSTRAINT FK_StorageFiles_Folders REFERENCES Folders (Id),
        Name                 NVARCHAR(255)         NOT NULL,
        MimeType             NVARCHAR(200)         NOT NULL,
        SizeBytes            BIGINT                NOT NULL,
        ContentHash          NVARCHAR(128)         NOT NULL,
        ObjectKey            NVARCHAR(500)         NOT NULL,
        ThumbnailObjectKey   NVARCHAR(500)         NULL,
        Provider             INT                   NOT NULL,
        FileType             INT                   NOT NULL,
        ThumbnailStatus      INT                   NOT NULL CONSTRAINT DF_StorageFiles_ThumbnailStatus DEFAULT (0),
        ConversionStatus     INT                   NOT NULL CONSTRAINT DF_StorageFiles_ConversionStatus DEFAULT (0),
        OcrStatus            INT                   NOT NULL CONSTRAINT DF_StorageFiles_OcrStatus DEFAULT (0),
        MetadataJson         NVARCHAR(MAX)         NULL,
        IsDeleted            BIT                   NOT NULL CONSTRAINT DF_StorageFiles_IsDeleted DEFAULT (0),
        DeletionTime         DATETIME2             NULL,
        CreationTime         DATETIME2             NOT NULL,
        CreatorId            BIGINT                NOT NULL CONSTRAINT DF_StorageFiles_CreatorId DEFAULT (0),
        LastModificationTime DATETIME2             NULL,
        LastModifierId       BIGINT                NULL
    );

    CREATE UNIQUE INDEX UX_StorageFiles_BusinessId ON StorageFiles (BusinessId);
    CREATE INDEX IX_StorageFiles_Application_Parent ON StorageFiles (ApplicationId, ParentFolderId) INCLUDE (Name, IsDeleted);
    CREATE INDEX IX_StorageFiles_Application_Hash ON StorageFiles (ApplicationId, ContentHash);
END
