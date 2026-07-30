IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'TrashItems')
BEGIN
    CREATE TABLE TrashItems
    (
        Id                     BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TrashItems PRIMARY KEY,
        BusinessId             UNIQUEIDENTIFIER      NOT NULL,
        ApplicationId          BIGINT                NOT NULL CONSTRAINT FK_TrashItems_Applications REFERENCES Applications (Id),
        ItemType               INT                   NOT NULL,
        ItemId                 BIGINT                NOT NULL,
        ItemName               NVARCHAR(255)         NOT NULL,
        OriginalParentFolderId BIGINT                NULL,
        IsRestored             BIT                   NOT NULL CONSTRAINT DF_TrashItems_IsRestored DEFAULT (0),
        IsPurged               BIT                   NOT NULL CONSTRAINT DF_TrashItems_IsPurged DEFAULT (0),
        PurgedTime             DATETIME2             NULL,
        CreationTime           DATETIME2             NOT NULL,
        CreatorId              BIGINT                NOT NULL CONSTRAINT DF_TrashItems_CreatorId DEFAULT (0),
        LastModificationTime   DATETIME2             NULL,
        LastModifierId         BIGINT                NULL
    );

    CREATE UNIQUE INDEX UX_TrashItems_BusinessId ON TrashItems (BusinessId);
    CREATE INDEX IX_TrashItems_Application_Item ON TrashItems (ApplicationId, ItemType, ItemId);
    CREATE INDEX IX_TrashItems_Purge ON TrashItems (IsPurged, CreationTime);
END
