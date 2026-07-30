using FileManager.Domain.Aggregates.TrashAgg;
using FileManager.Domain.Enumerations;
using FileManager.Domain.Repositories;
using FileManager.Infrastructure.Persistence.SqlServer.Connection;
using FileManager.Infrastructure.Persistence.SqlServer.Sql;
using FileManager.Infrastructure.Persistence.SqlServer.Tables;
using Microsoft.Data.SqlClient;

namespace FileManager.Infrastructure.Persistence.SqlServer.Repositories;

public sealed class TrashRepository(ISqlConnectionFactory connectionFactory) : ITrashRepository
{
    private static readonly SqlColumn[] SelectAll =
    [
        TrashItems.Id, TrashItems.BusinessId, TrashItems.ApplicationId, TrashItems.ItemType, TrashItems.ItemId,
        TrashItems.ItemName, TrashItems.OriginalParentFolderId, TrashItems.IsRestored, TrashItems.IsPurged,
        TrashItems.PurgedTime, TrashItems.CreationTime, TrashItems.CreatorId,
        TrashItems.LastModificationTime, TrashItems.LastModifierId
    ];

    public async Task<TrashItem?> GetAsync(long applicationId, long id, CancellationToken cancellationToken)
    {
        await using var conn = (SqlConnection)connectionFactory.CreateReadConnection();
        await conn.OpenAsync(cancellationToken);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"""
            SELECT {SqlSchema.Cols(SelectAll)} FROM {TrashItems.Table}
            WHERE {TrashItems.Id.Name} = {TrashItems.Id.Parameter} AND {TrashItems.ApplicationId.Name} = {TrashItems.ApplicationId.Parameter}
            """;
        cmd.Add(TrashItems.Id, id);
        cmd.Add(TrashItems.ApplicationId, applicationId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }

    public async Task<TrashItem?> GetByItemAsync(long applicationId, TrashItemType itemType, long itemId, CancellationToken cancellationToken)
    {
        await using var conn = (SqlConnection)connectionFactory.CreateReadConnection();
        await conn.OpenAsync(cancellationToken);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"""
            SELECT {SqlSchema.Cols(SelectAll)} FROM {TrashItems.Table}
            WHERE {TrashItems.ApplicationId.Name} = {TrashItems.ApplicationId.Parameter}
              AND {TrashItems.ItemType.Name} = {TrashItems.ItemType.Parameter}
              AND {TrashItems.ItemId.Name} = {TrashItems.ItemId.Parameter}
              AND {TrashItems.IsPurged.Name} = 0
            """;
        cmd.Add(TrashItems.ApplicationId, applicationId);
        cmd.Add(TrashItems.ItemType, (int)itemType);
        cmd.Add(TrashItems.ItemId, itemId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }

    public async Task<IReadOnlyList<TrashItem>> GetExpiredAsync(DateTime olderThan, CancellationToken cancellationToken)
    {
        await using var conn = (SqlConnection)connectionFactory.CreateReadConnection();
        await conn.OpenAsync(cancellationToken);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"""
            SELECT {SqlSchema.Cols(SelectAll)} FROM {TrashItems.Table}
            WHERE {TrashItems.IsPurged.Name} = 0 AND {TrashItems.CreationTime.Name} <= {TrashItems.CreationTime.Parameter}
            """;
        cmd.Add(TrashItems.CreationTime, olderThan);

        var items = new List<TrashItem>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            items.Add(Map(reader));

        return items;
    }

    public async Task InsertAsync(TrashItem trashItem, CancellationToken cancellationToken)
    {
        await using var conn = (SqlConnection)connectionFactory.CreateWriteConnection();
        await conn.OpenAsync(cancellationToken);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"""
            INSERT INTO {TrashItems.Table}
                ({TrashItems.BusinessId.Name}, {TrashItems.ApplicationId.Name}, {TrashItems.ItemType.Name}, {TrashItems.ItemId.Name},
                 {TrashItems.ItemName.Name}, {TrashItems.OriginalParentFolderId.Name}, {TrashItems.IsRestored.Name}, {TrashItems.IsPurged.Name},
                 {TrashItems.CreationTime.Name}, {TrashItems.CreatorId.Name})
            OUTPUT INSERTED.{TrashItems.Id.Name}
            VALUES
                ({TrashItems.BusinessId.Parameter}, {TrashItems.ApplicationId.Parameter}, {TrashItems.ItemType.Parameter}, {TrashItems.ItemId.Parameter},
                 {TrashItems.ItemName.Parameter}, {TrashItems.OriginalParentFolderId.Parameter}, {TrashItems.IsRestored.Parameter}, {TrashItems.IsPurged.Parameter},
                 {TrashItems.CreationTime.Parameter}, {TrashItems.CreatorId.Parameter})
            """;

        cmd.Add(TrashItems.BusinessId, (Guid)trashItem.BusinessId);
        cmd.Add(TrashItems.ApplicationId, trashItem.ApplicationId);
        cmd.Add(TrashItems.ItemType, (int)trashItem.ItemType);
        cmd.Add(TrashItems.ItemId, trashItem.ItemId);
        cmd.Add(TrashItems.ItemName, trashItem.ItemName);
        cmd.Add(TrashItems.OriginalParentFolderId, trashItem.OriginalParentFolderId);
        cmd.Add(TrashItems.IsRestored, trashItem.IsRestored);
        cmd.Add(TrashItems.IsPurged, trashItem.IsPurged);
        cmd.Add(TrashItems.CreationTime, trashItem.CreationTime);
        cmd.Add(TrashItems.CreatorId, trashItem.CreatorId);

        var generatedId = (long)(await cmd.ExecuteScalarAsync(cancellationToken))!;
        trashItem.AssignId(generatedId);
    }

    public async Task UpdateAsync(TrashItem trashItem, CancellationToken cancellationToken)
    {
        await using var conn = (SqlConnection)connectionFactory.CreateWriteConnection();
        await conn.OpenAsync(cancellationToken);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"""
            UPDATE {TrashItems.Table}
            SET {SqlSchema.SetClause(
                TrashItems.IsRestored, TrashItems.IsPurged, TrashItems.PurgedTime,
                TrashItems.LastModificationTime, TrashItems.LastModifierId)}
            WHERE {TrashItems.Id.Name} = {TrashItems.Id.Parameter} AND {TrashItems.ApplicationId.Name} = {TrashItems.ApplicationId.Parameter}
            """;

        cmd.Add(TrashItems.IsRestored, trashItem.IsRestored);
        cmd.Add(TrashItems.IsPurged, trashItem.IsPurged);
        cmd.Add(TrashItems.PurgedTime, trashItem.PurgedTime);
        cmd.Add(TrashItems.LastModificationTime, trashItem.LastModificationTime);
        cmd.Add(TrashItems.LastModifierId, trashItem.LastModifierId);
        cmd.Add(TrashItems.Id, trashItem.Id);
        cmd.Add(TrashItems.ApplicationId, trashItem.ApplicationId);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DeleteAsync(long applicationId, long id, CancellationToken cancellationToken)
    {
        await using var conn = (SqlConnection)connectionFactory.CreateWriteConnection();
        await conn.OpenAsync(cancellationToken);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"""
            DELETE FROM {TrashItems.Table}
            WHERE {TrashItems.Id.Name} = {TrashItems.Id.Parameter} AND {TrashItems.ApplicationId.Name} = {TrashItems.ApplicationId.Parameter}
            """;
        cmd.Add(TrashItems.Id, id);
        cmd.Add(TrashItems.ApplicationId, applicationId);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private static TrashItem Map(SqlDataReader reader) =>
        TrashItem.FromPersistence(
            id: reader.GetInt64(0),
            businessId: reader.GetGuid(1),
            applicationId: reader.GetInt64(2),
            itemType: (TrashItemType)reader.GetInt32(3),
            itemId: reader.GetInt64(4),
            itemName: reader.GetString(5),
            originalParentFolderId: reader.NullableLong(6),
            isRestored: reader.GetBoolean(7),
            isPurged: reader.GetBoolean(8),
            purgedTime: reader.NullableDateTime(9),
            creationTime: reader.GetDateTime(10),
            creatorId: reader.GetInt64(11),
            lastModificationTime: reader.NullableDateTime(12),
            lastModifierId: reader.NullableLong(13));
}
