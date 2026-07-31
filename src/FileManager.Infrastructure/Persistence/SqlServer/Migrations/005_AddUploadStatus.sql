IF NOT EXISTS (
    SELECT 1
    FROM sys.columns c
    INNER JOIN sys.tables t ON c.object_id = t.object_id
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE s.name = '{{Schema}}' AND t.name = 'StorageFiles' AND c.name = 'UploadStatus')
BEGIN
    ALTER TABLE [{{Schema}}].[StorageFiles]
        ADD UploadStatus INT NOT NULL CONSTRAINT DF_StorageFiles_UploadStatus DEFAULT (2);
END
