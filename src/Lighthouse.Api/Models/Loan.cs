namespace Lighthouse.Api.Models;

public sealed record Loan(string LoanId, string MemberId, decimal PrincipalBalance, decimal AnnualRate,
    DateOnly InterestAsOfDate, DateOnly DueDate, bool IsClosed);
