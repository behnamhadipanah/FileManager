IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = '{{Schema}}')
    EXEC(N'CREATE SCHEMA [{{Schema}}]');
