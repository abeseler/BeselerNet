using Beseler.ApiService.Application.Outbox;
using Dapper;
using Npgsql;
using System.Runtime.CompilerServices;

namespace Beseler.ApiService.Accounts;

public sealed class AccountRepository(NpgsqlDataSource db, OutboxRepository outbox)
{
    public async Task<Account?> GetByIdAsync(int id, CancellationToken stoppingToken = default)
    {
        await using var connection = await db.OpenConnectionAsync(stoppingToken);
        return await connection.QuerySingleOrDefaultAsync<Account>(
            """
            SELECT *
            FROM app.account
            WHERE account_id = @id;
            """, new { id });
    }

    public async Task<Account?> GetByEmailAsync(string email, CancellationToken stoppingToken = default)
    {
        await using var connection = await db.OpenConnectionAsync(stoppingToken);
        return await connection.QuerySingleOrDefaultAsync<Account>(
            """
            SELECT *
            FROM app.account
            WHERE email = @email;
            """, new { email });
    }

    public async Task<Result<Account>> SaveChangesAsync(Account account, CancellationToken stoppingToken = default)
    {
        var result = account switch
        {
            { AccountId: 0 } => await InsertAsync(account, stoppingToken),
            { HasUnsavedChanges: true } => await UpdateAsync(account, stoppingToken),
            _ => account
        };

        if (result.Failed) return result;
        if (account.DomainEvents.Count > 0)
        {
            var messages = account.DomainEvents.Select(OutboxMessage.CreateFrom);
            await outbox.Enqueue(messages, stoppingToken);
        }

        account.SavedChanges();

        return result;
    }

    private async Task<Result<Account>> InsertAsync(Account account, CancellationToken stoppingToken = default)
    {
        await using var connection = await db.OpenConnectionAsync(stoppingToken);
        var accountId = await connection.QuerySingleAsync<int>(
            """
            INSERT INTO app.account (
                email,
                given_name,
                family_name,
                secret_hash,
                created_on)
            VALUES (
                @Email,
                @GivenName,
                @FamilyName,
                @SecretHash,
                @CreatedOn)
            RETURNING account_id;
            """, account);

        account.SetIdentity(accountId);

        return account;
    }

    private async Task<Result<Account>> UpdateAsync(Account account, CancellationToken stoppingToken = default)
    {
        using var connection = await db.OpenConnectionAsync(stoppingToken);
        var result = await connection.ExecuteAsync(
            """
            UPDATE app.account
            SET
                email = @Email,
                given_name = @GivenName,
                family_name = @FamilyName,
                secret_hash = @SecretHash,
                is_locked = @IsLocked,
                is_verified = @IsVerified,
                last_login_on = @LastLoginOn,
                failed_login_attempts = @FailedLoginAttempts
            WHERE account_id = @AccountId;
            """
        , account);

        return result == 1 ? account : new Exception("Failed to update account.");
    }
}

file static class AccountMapper
{
    public static void SetIdentity(this Account account, int id) => SetAccountId(account, id);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = $"set_{nameof(Account.AccountId)}")]
    private static extern void SetAccountId(Account @this, int value);
}