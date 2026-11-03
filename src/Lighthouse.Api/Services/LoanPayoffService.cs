using Lighthouse.Api.Models;

namespace Lighthouse.Api.Services;

/// <summary>Produces fictional payoff quotes using the approved training business rules.</summary>
public sealed class LoanPayoffService
{
    private const int GraceDays = 15;

    /// <summary>Calculates one payoff quote using simple interest and the inclusive grace period.</summary>
    /// <param name="loan">The open loan and its decimal balance and annual rate.</param>
    /// <param name="member">The active member whose identifier matches the loan.</param>
    /// <param name="payoffDate">The requested calendar date; earlier as-of dates accrue zero new interest.</param>
    /// <returns>A quote with the unrounded interest component and once-rounded total.</returns>
    /// <exception cref="ArgumentNullException">The loan or member is null.</exception>
    /// <exception cref="ArgumentException">An identifier is missing or the member does not match the loan.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The balance, rate, or due date is outside the supported range.</exception>
    /// <exception cref="InvalidOperationException">The member is inactive or the loan is closed.</exception>
    /// <exception cref="OverflowException">A supplied decimal amount exceeds representable arithmetic.</exception>
    public PayoffQuote CalculatePayoff(Loan loan, Member member, DateOnly payoffDate)
    {
        ArgumentNullException.ThrowIfNull(loan);
        MemberValidator.Validate(member);
        ValidateLoan(loan, member);
        int rawInterestDays = payoffDate.DayNumber - loan.InterestAsOfDate.DayNumber;
        int interestDays = Math.Max(0, rawInterestDays);
        int rawDaysPastDue = payoffDate.DayNumber - loan.DueDate.DayNumber;
        int daysPastDue = Math.Max(0, rawDaysPastDue);
        DateOnly graceEnd = loan.DueDate.AddDays(GraceDays);
        bool withinGrace = payoffDate <= graceEnd;
        decimal interest = CalculateInterest(loan.PrincipalBalance, loan.AnnualRate, interestDays);
        decimal fee = CalculateLateFee(loan.PrincipalBalance, daysPastDue);
        decimal total = RoundTotal(loan.PrincipalBalance + interest + fee);
        var trace = BuildTrace(loan, rawInterestDays, rawDaysPastDue, withinGrace, interestDays, fee, interest, total);
        return new PayoffQuote(loan.LoanId, member.MemberId, loan.PrincipalBalance,
            interest, fee, total, interestDays, daysPastDue, GraceDays, graceEnd, withinGrace, trace);
    }

    private static decimal CalculateInterest(decimal principal, decimal rate, int days) =>
        principal == 0m ? 0m : principal * rate * days / 365m;

    private static decimal CalculateLateFee(decimal principal, int daysPastDue) =>
        principal > 0m && daysPastDue > GraceDays ? 25m : 0m;

    private static decimal RoundTotal(decimal total) => decimal.Round(total, 2, MidpointRounding.AwayFromZero);

    private static void ValidateLoan(Loan loan, Member member)
    {
        if (string.IsNullOrWhiteSpace(loan.LoanId)) throw new ArgumentException("Loan ID is required.", nameof(loan));
        if (loan.MemberId != member.MemberId) throw new ArgumentException("Loan does not belong to the supplied member.", nameof(loan));
        if (loan.IsClosed) throw new InvalidOperationException("Loan is closed.");
        if (loan.PrincipalBalance < 0m) throw new ArgumentOutOfRangeException(nameof(loan), "Principal cannot be negative.");
        if (loan.AnnualRate < 0m || loan.AnnualRate > 1m) throw new ArgumentOutOfRangeException(nameof(loan), "Annual rate must be between 0 and 1.");
        if (loan.DueDate > DateOnly.MaxValue.AddDays(-GraceDays)) throw new ArgumentOutOfRangeException(nameof(loan), "Due date cannot support the grace window.");
    }

    private static IReadOnlyList<string> BuildTrace(Loan loan, int rawInterestDays, int rawDaysPastDue,
        bool withinGrace, int interestDays, decimal fee, decimal interest, decimal total)
    {
        List<string> trace =
        [
            "Quote uses synthetic training data and is not a production payoff statement.",
            "Principal comes from the supplied loan record.",
            rawInterestDays < 0 ? "The payoff date precedes interest as-of; negative interest days are clamped to zero." :
                rawInterestDays == 0 ? "The payoff date equals interest as-of; no new interest accrues." :
                "Interest uses elapsed calendar days after the interest as-of date.",
            rawDaysPastDue < 0 ? "The requested payoff precedes the due date." :
                rawDaysPastDue == 0 ? "The requested payoff is on the due date." : "The requested payoff is after the due date.",
            withinGrace ? "The requested date is on or before the calculated grace-end date." :
                "The requested date is beyond the calculated grace-end date."
        ];
        if (loan.PrincipalBalance == 0m)
        {
            trace.Add("Zero principal produces a zero quote, with no interest and no late fee.");
            return trace.AsReadOnly();
        }
        trace.Add("Interest is simple interest using Actual/365 Fixed.");
        trace.Add("The denominator remains 365 in leap years.");
        trace.Add(loan.AnnualRate == 0m ? "The annual rate is zero; the interest component is zero." :
            interestDays == 0 ? "The interest-day count is zero; the interest component is zero." :
            "The interest component remains unrounded until the final total.");
        trace.Add(fee > 0m ? "The calculation applies the fixed late fee once." : "The calculation applies no late fee.");
        trace.Add(loan.PrincipalBalance + interest + fee == total ? "The total already has whole-cent precision." :
            "The total is rounded once to two decimal places.");
        trace.Add("No compounding, escrow, payoff charge, or payment posting is modeled.");
        trace.Add("A person must review the business-rule assumptions before using the quote.");
        return trace.AsReadOnly();
    }
}
