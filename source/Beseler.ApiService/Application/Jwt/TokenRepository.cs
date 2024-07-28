using Dapper;
using Npgsql;

namespace Beseler.ApiService.Application.Jwt;

internal sealed class TokenRepository(NpgsqlDataSource db)
{
    public async Task<TokenLog?> GetByIdAsync(Guid tokenId, CancellationToken stoppingToken = default)
    {
        await using var connection = await db.OpenConnectionAsync(stoppingToken);
        return await connection.QuerySingleOrDefaultAsync<TokenLog>("""
            SELECT *
            FROM app.token_log
            WHERE token_id = @tokenId;
            """, new { tokenId });
    }

    public async Task SaveChangesAsync(TokenLog log, CancellationToken stoppingToken = default)
    {
        var parameters = new DynamicParameters(log);
        using var connection = await db.OpenConnectionAsync(stoppingToken);
        _ = log.TokenLogId == 0 ?
            await connection.ExecuteAsync(InsertQuery, parameters) :
            await connection.ExecuteAsync(UpdateQuery, parameters);
    }

    public async Task RevokeTokensForAccount(int accountId, CancellationToken stoppingToken = default)
    {
        await using var connection = await db.OpenConnectionAsync(stoppingToken);
        await connection.ExecuteAsync("""
            UPDATE app.token_log
            SET is_revoked = true
            WHERE account_id = @accountId
                AND is_revoked = false
                AND expires_on > now() AT TIME ZONE 'utc'
            """, new { accountId });
    }

    public async Task RevokeChainAsync(Guid tokenId, CancellationToken stoppingToken = default)
    {
        await using var connection = await db.OpenConnectionAsync(stoppingToken);
        await connection.ExecuteAsync(RevokeTokenChainQuery, new { tokenId });
    }

    private const string InsertQuery = """
        INSERT INTO app.token_log
        (
            token_id,
            created_on,
            expires_on,
            is_revoked,
            account_id
        )
        VALUES
        (
            @TokenId,
            @CreatedOn,
            @ExpiresOn,
            @IsRevoked,
            @AccountId
        );
        """;

    private const string UpdateQuery = """
        UPDATE app.token_log
        SET replaced_by_token_id = @ReplacedByTokenId,
            is_revoked = @IsRevoked
        WHERE token_log_id = @TokenLogId;
        """;

    private const string RevokeTokenChainQuery = """
        WITH RECURSIVE descendants AS (
            SELECT
                token_log_id,
                token_id,
                replaced_by_token_id
            FROM app.token_log WHERE token_id = @tokenId
            UNION ALL
            SELECT
                tl.token_log_id,
                tl.token_id,
                tl.replaced_by_token_id
            FROM app.token_log tl
            INNER JOIN descendants td
                ON td.replaced_by_token_id = tl.token_id
            WHERE tl.is_revoked = false
                AND tl.expires_on > now() AT TIME ZONE 'utc'
        ),
        ancestors AS (
            SELECT
                token_log_id,
                token_id,
                replaced_by_token_id
            FROM app.token_log WHERE replaced_by_token_id = @tokenId
            UNION ALL
            SELECT
                tl.token_log_id,
                tl.token_id,
                tl.replaced_by_token_id
            FROM app.token_log tl
            INNER JOIN ancestors ta
                ON ta.token_id = tl.replaced_by_token_id
            WHERE tl.is_revoked = false
                AND tl.expires_on > now() AT TIME ZONE 'utc'
        ),
        ids AS (
            SELECT ta.token_log_id FROM ancestors ta
            UNION
            SELECT td.token_log_id FROM descendants td
        )
        UPDATE app.token_log tl
        SET is_revoked = true
        FROM ids i
        WHERE i.token_log_id = tl.token_log_id
        """;
}
