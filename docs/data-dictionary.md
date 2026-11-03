# Synthetic data dictionary

The SQLite file `src/Lighthouse.Api/Data/lighthouse.db` contains exactly 500 fictional members, 1,200 accounts, and 300 loans. `seed.sql` reconstructs the same schema and data in a new empty database. The database is opened in read-only mode by the API. The names, financial amounts, and dates are invented and have no relationship to SchoolsFirst records.

| Table | Fields and interpretation |
| --- | --- |
| Members | MemberId is M plus four digits; FullName is a synthetic name; Segment is Educator, Staff, Family, or Retiree; JoinedDate is ISO calendar date; IsActive is 0 or 1. |
| Accounts | AccountId is a synthetic record key, not a real account number; MemberId is a foreign key; AccountType is Checking, Savings, or Certificate; BalanceCents is an integer; IsOpen is 0 or 1. |
| Loans | LoanId is L plus four digits; MemberId is a foreign key; PrincipalCents is an integer; AnnualRate is invariant decimal text; InterestAsOfDate and DueDate are ISO dates; IsClosed is 0 or 1. |

Money is stored as integer cents and converted to decimal in C#. Rates are stored as text to avoid SQLite floating-point currency loss. These choices support reproducible calculations; they are not a complete production banking data model.

## Stable rehearsal fixtures

| Fixture | Meaning |
| --- | --- |
| L0001 / M0001 | Active member, open loan, principal 10000.00, rate 0.073, interest as-of 2026-10-01, due 2026-10-31. A payoff on 2026-10-31 is 10060.00. |
| L0298 / M0298 | Open zero-balance loan. |
| L0299 / M0499 | Closed loan associated with an active member; the completed endpoint returns 409. |
| L0300 / M0500 | Loan associated with the inactive member; the completed endpoint returns 409. |
| L9999 | Valid-shaped ID that does not exist; the completed endpoint returns 404. |

Do not use these rows as the only business-rule test data. Construct direct Loan and Member records to exercise the exact grace and rounding boundaries independently. The search endpoint intentionally uses unsafe concatenated SQL in the starter and is reserved for the facilitator's security demonstration.
