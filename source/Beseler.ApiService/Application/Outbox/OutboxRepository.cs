using Dapper;
using Npgsql;

namespace Beseler.ApiService.Application.Outbox;

public sealed class OutboxRepository(NpgsqlDataSource db)
{
    public const string ServiceId = "Beseler.ApiService";

    public async Task Enqueue(IEnumerable<OutboxMessage> messages, CancellationToken stoppingToken)
        => await TaskExt.WhenAll(messages.Select(message => Enqueue(message, stoppingToken)).ToArray());

    public async Task Enqueue(OutboxMessage message, CancellationToken stoppingToken)
    {
        using var connection = await db.OpenConnectionAsync(stoppingToken);
        _ = await connection.ExecuteAsync("""
            INSERT INTO app.outbox_queue (
                service_id,
                payload,
                receives_remaining
            )
            VALUES (
                @ServiceId,
                @Payload::json,
                @ReceivesRemaining
            )
            """, new
        {
            ServiceId,
            message.Payload,
            message.ReceivesRemaining
        });
    }

    public async Task<OutboxMessage?> Dequeue(CancellationToken stoppingToken)
    {
        using var connection = await db.OpenConnectionAsync(stoppingToken);
        return await connection.QuerySingleOrDefaultAsync<OutboxMessage>("""
            WITH messages AS (
                SELECT id
                FROM app.outbox_queue
                WHERE service_id = @ServiceId
                AND receives_remaining > 0
                AND invisible_until <= NOW() AT TIME ZONE 'utc'
                LIMIT 1
                FOR UPDATE SKIP LOCKED
            )
            UPDATE app.outbox_queue o
            SET invisible_until = NOW() AT TIME ZONE 'utc' + interval '30 seconds',
                receives_remaining = receives_remaining - 1
            FROM messages m
            WHERE m.id = o.id
            RETURNING *
            """, new { ServiceId });
    }

    public async Task DeleteAsync(OutboxMessage message, CancellationToken stoppingToken)
    {
        using var connection = await db.OpenConnectionAsync(stoppingToken);
        _ = await connection.ExecuteAsync("DELETE FROM app.outbox_queue WHERE id = @Id", new { message.Id });
    }
}
