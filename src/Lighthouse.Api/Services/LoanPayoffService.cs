using Lighthouse.Api.Models;

namespace Lighthouse.Api.Services;

public sealed class LoanPayoffService
{
    public PayoffQuote CalculatePayoff(Loan loan, Member member, DateOnly payoffDate)
    {
        ArgumentNullException.ThrowIfNull(loan);
        ArgumentNullException.ThrowIfNull(member);

        if (string.IsNullOrWhiteSpace(member.MemberId))
        {
            throw new ArgumentException("Member ID is required.", nameof(member));
        }

        if (!member.IsActive)
        {
            throw new InvalidOperationException("Member is inactive.");
        }

        if (string.IsNullOrWhiteSpace(loan.LoanId))
        {
            throw new ArgumentException("Loan ID is required.", nameof(loan));
        }

        if (loan.MemberId != member.MemberId)
        {
            throw new ArgumentException("Loan does not belong to the supplied member.", nameof(loan));
        }

        if (loan.IsClosed)
        {
            throw new InvalidOperationException("Loan is closed.");
        }

        if (loan.PrincipalBalance < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(loan), "Principal cannot be negative.");
        }

        if (loan.AnnualRate < 0m || loan.AnnualRate > 1m)
        {
            throw new ArgumentOutOfRangeException(nameof(loan), "Annual rate must be between 0 and 1.");
        }

        if (loan.DueDate > DateOnly.MaxValue.AddDays(-15))
        {
            throw new ArgumentOutOfRangeException(nameof(loan), "Due date cannot support the grace window.");
        }

        List<string> trace = [];
        trace.Add("Quote uses synthetic training data and is not a production payoff statement.");
        trace.Add("Principal comes from the supplied loan record.");

        int rawInterestDays = payoffDate.DayNumber - loan.InterestAsOfDate.DayNumber;
        int interestDays = Math.Max(0, rawInterestDays);
        int rawDaysPastDue = payoffDate.DayNumber - loan.DueDate.DayNumber;
        int daysPastDue = Math.Max(0, rawDaysPastDue);
        int gracePeriodDays = 10; // RULE_GRACE_DAYS: compare this with the approved rules.
        DateOnly graceEndDate = loan.DueDate.AddDays(gracePeriodDays);

        if (rawInterestDays < 0)
        {
            trace.Add("The payoff date precedes interest as-of; negative interest days are clamped to zero.");
        }
        else if (rawInterestDays == 0)
        {
            trace.Add("The payoff date equals interest as-of; no new interest accrues.");
        }
        else
        {
            trace.Add("Interest uses elapsed calendar days after the interest as-of date.");
        }

        if (rawDaysPastDue < 0)
        {
            trace.Add("The requested payoff precedes the due date.");
        }
        else if (rawDaysPastDue == 0)
        {
            trace.Add("The requested payoff is on the due date.");
        }
        else
        {
            trace.Add("The requested payoff is after the due date.");
        }

        bool isWithinGracePeriod = payoffDate <= graceEndDate;
        if (isWithinGracePeriod)
        {
            trace.Add("The requested date is on or before the calculated grace-end date.");
        }
        else
        {
            trace.Add("The requested date is beyond the calculated grace-end date.");
        }

        if (loan.PrincipalBalance == 0m)
        {
            trace.Add("Zero principal produces a zero quote, with no interest and no late fee.");
            return new PayoffQuote(
                loan.LoanId,
                member.MemberId,
                0m,
                0m,
                0m,
                0m,
                interestDays,
                daysPastDue,
                gracePeriodDays,
                graceEndDate,
                isWithinGracePeriod,
                trace.AsReadOnly());
        }

        const decimal dayCountDenominator = 365m;
        decimal principal = loan.PrincipalBalance;
        decimal annualRate = loan.AnnualRate;
        decimal rawInterest = principal * annualRate * interestDays / dayCountDenominator;
        trace.Add("Interest is simple interest using Actual/365 Fixed.");
        trace.Add("The denominator remains 365 in leap years.");

        if (annualRate == 0m)
        {
            trace.Add("The annual rate is zero; the interest component is zero.");
        }
        else if (interestDays == 0)
        {
            trace.Add("The interest-day count is zero; the interest component is zero.");
        }
        else
        {
            trace.Add("The interest component remains unrounded until the final total.");
        }

        decimal lateFee = 0m;
        if (daysPastDue >= gracePeriodDays) // RULE_GRACE_BOUNDARY: check the final included day.
        {
            lateFee = 25m;
            trace.Add("The calculation applies the fixed late fee once.");
        }
        else
        {
            trace.Add("The calculation applies no late fee.");
        }

        decimal unroundedTotal = principal + rawInterest + lateFee;
        decimal totalPayoff = decimal.Round(unroundedTotal, 2, MidpointRounding.ToEven); // ROUNDING_DECISION

        if (unroundedTotal == totalPayoff)
        {
            trace.Add("The total already has whole-cent precision.");
        }
        else
        {
            trace.Add("The total is rounded once to two decimal places.");
        }

        trace.Add("No compounding, escrow, payoff charge, or payment posting is modeled.");
        trace.Add("A person must review the business-rule assumptions before using the quote.");

        return new PayoffQuote(
            loan.LoanId,
            member.MemberId,
            principal,
            rawInterest,
            lateFee,
            totalPayoff,
            interestDays,
            daysPastDue,
            gracePeriodDays,
            graceEndDate,
            isWithinGracePeriod,
            trace.AsReadOnly());
    }
}
