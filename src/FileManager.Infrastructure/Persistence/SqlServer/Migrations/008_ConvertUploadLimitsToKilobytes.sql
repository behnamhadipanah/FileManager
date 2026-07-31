-- Upload limit columns store kilobytes (registration API values). Convert legacy byte values once.
IF EXISTS (
    SELECT 1
    FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE s.name = '{{Schema}}' AND t.name = 'Applications')
BEGIN
    UPDATE [{{Schema}}].[Applications]
    SET MinImageSize = MinImageSize / 1024,
        MaxImageSize = MaxImageSize / 1024,
        MinVideoSize = MinVideoSize / 1024,
        MaxVideoSize = MaxVideoSize / 1024,
        MinDocumentSize = MinDocumentSize / 1024,
        MaxDocumentSize = MaxDocumentSize / 1024
    -- Only convert values that are clearly stored as bytes (>= 1 MB). Kilobyte limits stay unchanged.
    WHERE MaxImageSize >= 1048576
       OR MaxVideoSize >= 1048576
       OR MaxDocumentSize >= 1048576;
END
