using Lighthouse.Api.Models;
using Lighthouse.Api.Services;
using Xunit;

namespace Lighthouse.Tests;

public sealed class LoanPayoffServiceTests
{
    private static readonly Member ActiveMember = new("M0001", "Avery Bennett", "Educator", new DateOnly(2012, 1, 1), true);
    private readonly LoanPayoffService service = new();
    private static Loan Loan(decimal principal = 1000m, decimal rate = 0.0365m,
        DateOnly? asOf = null, DateOnly? due = null) => new("L0001", "M0001", principal, rate,
            asOf ?? new DateOnly(2026, 10, 1), due ?? new DateOnly(2026, 10, 1), false);

    [Fact]
    public void CalculatePayoff_StandardQuote_ReturnsPrincipalPlusSimpleInterest()
    {
        var loan = Loan(10000m, 0.073m, due: new DateOnly(2026, 10, 31));
        var quote = service.CalculatePayoff(loan, ActiveMember, new DateOnly(2026, 10, 11));
        Assert.Equal(20m, quote.InterestAccrued);
        Assert.Equal(0m, quote.LateFee);
        Assert.Equal(10020m, quote.TotalPayoff);
    }

    [Fact]
    public void CalculatePayoff_OnDueDate_ChargesNoLateFee()
    {
        var quote = service.CalculatePayoff(Loan(due: new DateOnly(2026, 10, 11)), ActiveMember, new DateOnly(2026, 10, 11));
        Assert.Equal(1m, quote.InterestAccrued);
        Assert.Equal(0m, quote.LateFee);
        Assert.Equal(1001m, quote.TotalPayoff);
    }

    [Theory]
    [InlineData(9, "1000.90", "0")]
    [InlineData(10, "1001.00", "0")]
    [InlineData(11, "1001.10", "0")]
    [InlineData(14, "1001.40", "0")]
    [InlineData(15, "1001.50", "0")]
    [InlineData(16, "1026.60", "25")]
    public void CalculatePayoff_GraceBoundary_AppliesFeeOnlyAfterFinalDay(int days, string expectedTotal, string expectedFee)
    {
        // Constants are independent business-rule expectations, not calculated by the subject.
        var quote = service.CalculatePayoff(Loan(), ActiveMember, new DateOnly(2026, 10, 1).AddDays(days));
        Assert.Equal(decimal.Parse(expectedTotal, System.Globalization.CultureInfo.InvariantCulture), quote.TotalPayoff);
        Assert.Equal(decimal.Parse(expectedFee, System.Globalization.CultureInfo.InvariantCulture), quote.LateFee);
    }

    [Fact]
    public void CalculatePayoff_HalfCentMidpoint_RoundsFinalTotalAwayFromZero()
    {
        var loan = Loan(100m, 0.01825m, due: new DateOnly(2026, 10, 31));
        var quote = service.CalculatePayoff(loan, ActiveMember, new DateOnly(2026, 10, 2));
        Assert.Equal(0.005m, quote.InterestAccrued);
        Assert.Equal(100.01m, quote.TotalPayoff);
    }

    [Fact]
    public void CalculatePayoff_ZeroBalanceLate_ReturnsZeroForAllAmounts()
    {
        var quote = service.CalculatePayoff(Loan(0m, 0.073m), ActiveMember, new DateOnly(2026, 11, 1));
        Assert.Equal(0m, quote.PrincipalBalance);
        Assert.Equal(0m, quote.InterestAccrued);
        Assert.Equal(0m, quote.LateFee);
        Assert.Equal(0m, quote.TotalPayoff);
    }

    [Fact]
    public void CalculatePayoff_BeforeInterestAsOf_ClampsInterestDaysToZero()
    {
        var loan = Loan(asOf: new DateOnly(2026, 10, 10), due: new DateOnly(2026, 10, 31));
        var quote = service.CalculatePayoff(loan, ActiveMember, new DateOnly(2026, 10, 1));
        Assert.Equal(0, quote.InterestDays);
        Assert.Equal(0m, quote.InterestAccrued);
        Assert.Equal(1000m, quote.TotalPayoff);
    }

    [Fact]
    public void CalculatePayoff_LeapYear_UsesFixed365DayDenominator()
    {
        var loan = Loan(1000m, 0.073m, new DateOnly(2024, 2, 28), new DateOnly(2024, 3, 31));
        var quote = service.CalculatePayoff(loan, ActiveMember, new DateOnly(2024, 3, 1));
        Assert.Equal(2, quote.InterestDays);
        Assert.Equal(0.4m, quote.InterestAccrued);
        Assert.Equal(1000.4m, quote.TotalPayoff);
    }

    [Fact]
    public void CalculatePayoff_GraceMetadata_UsesFifteenInclusiveDays()
    {
        var quote = service.CalculatePayoff(Loan(), ActiveMember, new DateOnly(2026, 10, 16));
        Assert.Equal(15, quote.GracePeriodDays);
        Assert.Equal(new DateOnly(2026, 10, 16), quote.GraceEndDate);
        Assert.True(quote.IsWithinGracePeriod);
    }

    [Fact]
    public void CalculatePayoff_GuideStandardExample_Returns1002()
    {
        var quote = service.CalculatePayoff(Loan(1000m, 0.073m, due: new DateOnly(2026, 11, 1)), ActiveMember, new DateOnly(2026, 10, 11));
        Assert.Equal(2m, quote.InterestAccrued);
        Assert.Equal(1002m, quote.TotalPayoff);
    }

    [Theory]
    [InlineData(10, "1000")]
    [InlineData(20, "1000")]
    [InlineData(25, "1000")]
    [InlineData(26, "1025")]
    public void CalculatePayoff_GuideZeroRateBoundary_UsesDocumentedFee(int octoberDay, string expected)
    {
        var quote = service.CalculatePayoff(Loan(1000m, 0m, due: new DateOnly(2026, 10, 10)), ActiveMember, new DateOnly(2026, 10, octoberDay));
        Assert.Equal(decimal.Parse(expected, System.Globalization.CultureInfo.InvariantCulture), quote.TotalPayoff);
    }

    [Fact]
    public void CalculatePayoff_InactiveMember_ThrowsInvalidOperation() =>
        Assert.Throws<InvalidOperationException>(() => service.CalculatePayoff(Loan(), ActiveMember with { IsActive = false }, new DateOnly(2026, 10, 1)));

    [Fact]
    public void CalculatePayoff_ClosedLoan_ThrowsInvalidOperation() =>
        Assert.Throws<InvalidOperationException>(() => service.CalculatePayoff(Loan() with { IsClosed = true }, ActiveMember, new DateOnly(2026, 10, 1)));

    [Fact]
    public void CalculatePayoff_NegativePrincipal_ThrowsArgumentOutOfRange() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => service.CalculatePayoff(Loan(-1m), ActiveMember, new DateOnly(2026, 10, 1)));

    [Fact]
    public void CalculatePayoff_MismatchedMember_ThrowsArgument() =>
        Assert.Throws<ArgumentException>(() => service.CalculatePayoff(Loan() with { MemberId = "M0002" }, ActiveMember, new DateOnly(2026, 10, 1)));

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public void CalculatePayoff_InvalidAnnualRate_ThrowsArgumentOutOfRange(int rate) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => service.CalculatePayoff(Loan(rate: rate), ActiveMember, new DateOnly(2026, 10, 1)));

    [Fact]
    public void CalculatePayoff_NullLoan_ThrowsArgumentNull() =>
        Assert.Throws<ArgumentNullException>(() => service.CalculatePayoff(null!, ActiveMember, new DateOnly(2026, 10, 1)));
}
