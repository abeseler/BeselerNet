using Dapper;
using Npgsql;

namespace Beseler.ApiService.Queues;

public sealed class QueueRepository(NpgsqlDataSource db)
{
    public async Task<MessageQueue?> GetByIdAsync(int queueId, CancellationToken stoppingToken)
    {
        await using var connection = await db.OpenConnectionAsync(stoppingToken);
        return await connection.QuerySingleOrDefaultAsync<MessageQueue>(
            """
            SELECT *
            FROM app.queue
            WHERE queue_id = @queueId;
            """, new { queueId });
    }

    public async Task<MessageQueue?> GetByNameAsync(string queueName, CancellationToken stoppingToken)
    {
        await using var connection = await db.OpenConnectionAsync(stoppingToken);
        return await connection.QuerySingleOrDefaultAsync<MessageQueue>(
            """
            SELECT *
            FROM app.queue
            WHERE queue_name = @queueName;
            """, new { queueName });
    }

    public async Task EnqueueAsync(QueuedMessage message, CancellationToken stoppingToken)
    {
        await using var connection = await db.OpenConnectionAsync(stoppingToken);
        await connection.ExecuteAsync(
            """
            INSERT INTO app.queue_message (id, queue_id, payload, created_on, expires_on, invisible_until, receive_count)
            VALUES (@Id, @QueueId, @Payload, @CreatedOn, @ExpiresOn, @InvisibleUntil, @ReceiveCount);
            """, new
            {
                message.Id,
                message.QueueId,
                message.Payload,
                message.CreatedOn,
                message.ExpiresOn,
                message.InvisibleUntil,
                message.ReceiveCount
            });
    }

    public async Task<List<QueuedMessage>> DequeueAsync(MessageQueue queue, int maxMessages, DateTime invisibleUntil, CancellationToken stoppingToken)
    {
        await using var connection = await db.OpenConnectionAsync(stoppingToken);
        var messages = await connection.QueryAsync<QueuedMessage>(
            """
            WITH messages AS (
                SELECT id
                FROM app.queue_message
                WHERE queue_id = @QueueId
                AND expires_on > NOW() AT TIME ZONE 'utc'
                AND invisible_until <= NOW() AT TIME ZONE 'utc'
                LIMIT @MaxMessages
                FOR UPDATE SKIP LOCKED
            )
            UPDATE app.queue_message o
            SET invisible_until = @InvisibleUntil,
                receive_count = receive_count + 1
            FROM messages m
            WHERE m.id = o.id
            RETURNING *
            """, new
            {
                queue.QueueId,
                maxMessages,
                invisibleUntil
            });

        var messageList = new List<QueuedMessage>();

        foreach (var message in messages)
        {
            if (message.ReceiveCount <= queue.MessageMaxReceives)
            {
                messageList.Add(message);
                continue;
            }

            if (queue.DeadLetterQueueId is not null && await GetByIdAsync(queue.DeadLetterQueueId.Value, stoppingToken) is { } dlq)
            {
                var expiresOn = DateTime.UtcNow.AddDays(dlq.MessageRetentionDays);
                await connection.ExecuteAsync(
                    """
                    UPDATE INTO app.queue_message
                    SET queue_id = @QueueId,
                        expires_on = @ExpiresOn,
                        invisible_until = NULL,
                        receive_count = 0
                    WHERE id = @Id;
                    """, new
                    {
                        message.Id,
                        QueueId = queue.DeadLetterQueueId,
                        ExpiresOn = expiresOn
                    });
            }
            else
            {
                await connection.ExecuteAsync(
                    """
                    DELETE FROM app.queue_message
                    WHERE id = @Id;
                    """, new { message.Id });                
            }
        }

        return messageList;
    }

    public async Task DeleteMessageAsync(MessageQueue queue, Guid messageId, int receiveCount, CancellationToken stoppingToken)
    {
        await using var connection = await db.OpenConnectionAsync(stoppingToken);
        await connection.ExecuteAsync(
            """
            DELETE FROM app.queue_message
            WHERE queue_id = @QueueId
            AND id = @MessageId
            AND receiveCount = @ReceiveCount;
            """, new { queue.QueueId, messageId, receiveCount });
    }
}
