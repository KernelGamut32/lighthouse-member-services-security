namespace Lighthouse.Api.Models;

public sealed record PayoffQuote(
    string LoanId,
    string MemberId,
    decimal PrincipalBalance,
    decimal InterestAccrued,
    decimal LateFee,
    decimal TotalPayoff,
    int InterestDays,
    int DaysPastDue,
    int GracePeriodDays,
    DateOnly GraceEndDate,
    bool IsWithinGracePeriod,
    IReadOnlyList<string> CalculationTrace);
