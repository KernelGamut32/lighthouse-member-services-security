namespace Lighthouse.Api.Models;

public sealed record Account(string AccountId, string MemberId, string AccountType, decimal Balance, bool IsOpen);
