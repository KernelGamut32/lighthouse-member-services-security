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

    private static IResult GetPayoff(string loanId, string? payoffDate,
        LoanRepository loans, MemberService members, LoanPayoffService payoff)
    {
        if (loanId.Length != 5 || loanId[0] != 'L' || loanId.AsSpan(1).IndexOfAnyExceptInRange('0', '9') >= 0)
            return Results.Problem(statusCode: 400, title: "Invalid loan ID", detail: "Use L followed by four ASCII digits.");
        if (string.IsNullOrWhiteSpace(payoffDate) ||
            !DateOnly.TryParseExact(payoffDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly date))
            return Results.Problem(statusCode: 400, title: "Invalid payoff date", detail: "A payoffDate in yyyy-MM-dd format is required.");
        var loan = loans.Find(loanId);
        if (loan is null) return Results.Problem(statusCode: 404, title: "Loan not found");
        try
        {
            var member = members.GetRequiredActiveMember(loan.MemberId);
            return Results.Ok(payoff.CalculatePayoff(loan, member, date));
        }
        catch (InvalidOperationException)
        {
            return Results.Problem(statusCode: 409, title: "Quote unavailable", detail: "The loan must be open and the member active.");
        }
    }
}
