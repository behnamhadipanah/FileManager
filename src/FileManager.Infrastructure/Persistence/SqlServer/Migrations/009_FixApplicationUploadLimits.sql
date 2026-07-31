-- Restore intended image upload limits (kilobytes) for applications affected by legacy conversion.
IF EXISTS (
    SELECT 1
    FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE s.name = '{{Schema}}' AND t.name = 'Applications')
BEGIN
    UPDATE [{{Schema}}].[Applications]
    SET MinImageSize = 1,
        MaxImageSize = 200000
    WHERE MaxImageSize = 20
      AND MinImageSize = 0;
END
