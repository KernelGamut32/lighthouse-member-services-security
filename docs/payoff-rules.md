# Loan payoff rules

## Authority and scope

This is the product owner's approved rule set for the fictional Lighthouse training API. Use it as the test oracle. Implementation behavior is not authority when it disagrees with these rules. Evidence date: October 29, 2026. Nothing here is a real credit union policy or financial advice.

## Inputs and eligibility

- A quote has a Loan, its matching Member, and a payoffDate. Dates have calendar-day precision with no time-zone conversion.
- The member must be active and its MemberId must match the loan. A closed loan cannot receive a quote.
- PrincipalBalance must be nonnegative. AnnualRate is a decimal fraction from 0 through 1 inclusive, such as 0.0725 for 7.25%.
- The due date must allow a 15-day addition within DateOnly's representable range.
- A quote before InterestAsOfDate is allowed and has zero newly accrued interest days. Do not add a new date-range restriction during the endpoint exercise.

## Interest and total

1. InterestDays = max(0, payoffDate minus InterestAsOfDate in calendar days).
2. InterestAccrued = PrincipalBalance * AnnualRate * InterestDays / 365. Use decimal arithmetic and a fixed 365-day denominator, including leap years.
3. GraceEndDate = DueDate plus 15 calendar days. The grace period includes the entire final day.
4. For positive principal, apply a single $25 late fee only when payoffDate is after GraceEndDate. DueDate + 15 has no fee; DueDate + 16 has a fee.
5. With zero principal, return zero interest, zero fee, and zero total even when the requested date is late.
6. TotalPayoff = round(PrincipalBalance + InterestAccrued + LateFee, 2, MidpointRounding.AwayFromZero). Round the total once. Do not round the interest component first.
7. No compounding, escrow, payment posting, tax, or extra payoff fee is modeled.

## Independent examples

All examples use an active member and an open loan. Set the interest as-of date and payoff date exactly as listed, rather than using today's date.

| Scenario | Principal | Rate | Interest as of | Due | Payoff | Expected interest | Fee | Total |
| --- | ---: | ---: | --- | --- | --- | ---: | ---: | ---: |
| Standard ten-day quote | 10000.00 | 0.073 | 2026-10-01 | 2026-10-31 | 2026-10-11 | 20.00 | 0.00 | 10020.00 |
| Due date | 1000.00 | 0.0365 | 2026-10-01 | 2026-10-11 | 2026-10-11 | 1.00 | 0.00 | 1001.00 |
| Day 14 after due | 1000.00 | 0.0365 | 2026-10-01 | 2026-10-01 | 2026-10-15 | 1.40 | 0.00 | 1001.40 |
| Last grace day | 1000.00 | 0.0365 | 2026-10-01 | 2026-10-01 | 2026-10-16 | 1.50 | 0.00 | 1001.50 |
| First late day | 1000.00 | 0.0365 | 2026-10-01 | 2026-10-01 | 2026-10-17 | 1.60 | 25.00 | 1026.60 |
| Rounding midpoint | 100.00 | 0.01825 | 2026-10-01 | 2026-10-31 | 2026-10-02 | 0.005 | 0.00 | 100.01 |
| Zero balance late | 0.00 | 0.073 | 2026-10-01 | 2026-10-01 | 2026-11-01 | 0.00 | 0.00 | 0.00 |

The midpoint example is deliberate: 100.005 rounds to 100.01 under AwayFromZero and to 100.00 under ToEven. This is a training business choice, not a claim that one rounding mode is universally correct for financial services.

## HTTP boundary contract for the separate agent task

- GET `/api/loans/{loanId}/payoff?payoffDate=YYYY-MM-DD`.
- A syntactically valid loan ID is L followed by four ASCII digits. Missing or malformed payoffDate, and malformed loanId, return HTTP 400 ProblemDetails.
- A valid but unknown loan returns HTTP 404 ProblemDetails.
- An inactive member or closed loan returns HTTP 409 ProblemDetails.
- An eligible quote returns HTTP 200 and the existing PayoffQuote fields.
- The learner starter deliberately does not yet meet all of this HTTP validation contract. Do not change service arithmetic or test expectations while implementing it.
