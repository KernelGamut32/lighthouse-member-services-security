using System.Globalization;
using Lighthouse.Api.Data;
using Lighthouse.Api.Services;

namespace Lighthouse.Api.Endpoints;

public static class LoanPayoffEndpoints
{
    public static IEndpointRouteBuilder MapLoanPayoffEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/loans/{loanId}/payoff", GetPayoff);
        return app;
    }

    // Cloud-agent task scope: validate this HTTP boundary without changing payoff arithmetic.
    private static IResult GetPayoff(string loanId, string? payoffDate,
        LoanRepository loans, MemberService members, LoanPayoffService payoff)
    {
        var loan = loans.Find(loanId);
        if (loan is null) return Results.NotFound(new { message = "Loan not found." });
        DateOnly date = DateOnly.Parse(payoffDate ?? string.Empty, CultureInfo.InvariantCulture);
        var member = members.GetRequiredActiveMember(loan.MemberId);
        return Results.Ok(payoff.CalculatePayoff(loan, member, date));
    }
}
