﻿using Beseler.ApiService.Accounts;

namespace Beseler.ApiService.Application.Jwt;

internal sealed record TokenLog
{
    private TokenLog() { }
    public TokenLog(Guid tokenId, DateTime expiresOn, Account account)
    {
        TokenId = tokenId;
        CreatedOn = DateTime.UtcNow;
        ExpiresOn = expiresOn;
        AccountId = account.AccountId;
    }
    public int TokenLogId { get; private set; }
    public Guid TokenId { get; private set; }
    public Guid? ReplacedByTokenId { get; private set; }
    public DateTime CreatedOn { get; private set; }
    public DateTime ExpiresOn { get; private set; }
    public bool IsRevoked { get; private set; }
    public int AccountId { get; private set; }
    public bool HasBeenReplaced => ReplacedByTokenId is not null;
    public void ReplacedBy(Guid newTokenId) => ReplacedByTokenId = newTokenId;
    public void Revoke() => IsRevoked = true;
}
