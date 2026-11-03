> **FACILITATOR SECURITY COPY.** This repository adds an intentionally affected dependency manifest outside the application solution. Read [SECURITY_DEMO_ONLY.md](SECURITY_DEMO_ONLY.md) before use. Live GitHub settings, alerts, PRs, and campaigns require administrator rehearsal.

# Lighthouse Member Services

Lighthouse is a fictional credit-union API used in the SchoolsFirst Federal Credit Union GitHub Copilot lab. Its people, accounts, loan records, and financial figures are invented. Participants explain code, establish correct behavior with independent tests, refactor safely, and review security evidence.

## Prepare a local workstation

Before class, install the lab's .NET 10 SDK **10.0.403**, Git, and VS Code. Use Windows PowerShell or the normal macOS terminal. Install the VS Code C# and GitHub Copilot extensions, then sign in with the GitHub account assigned to the workshop. The repository includes extension recommendations in `.vscode/extensions.json`.

Clone the assigned private repository using VS Code's **Git: Clone** command or your normal Git workflow. Open the repository folder that contains `Lighthouse.sln` and `global.json`. Confirm the repository owner, branch, and GitHub identity before editing. Initial package restore and Copilot/GitHub activity need network access.

Run these commands from that repository root. They are identical in PowerShell, zsh, and Bash:

```text
dotnet --version
dotnet restore Lighthouse.sln --locked-mode
dotnet test Lighthouse.sln --no-restore
```

The SDK selection comes from `global.json`. At the untouched learner starting point, expect one passing smoke test. That result confirms application startup only. Exercise 2 supplies the missing business-rule tests; later checkpoints and the completed reference include additional tests. If SDK selection, package restore, or account access fails, take the prepared partner-station route and have the administrator resolve the readiness issue.

## Run the application and verify the sample data

In the repository-root terminal, run:

```text
dotnet run --project src/Lighthouse.Api --no-launch-profile
```

Keep that terminal running. Open these addresses in your browser:

- Health: `http://127.0.0.1:5080/health`
- Sample payoff: `http://127.0.0.1:5080/api/loans/L0001/payoff?payoffDate=2026-10-31`

A successful sample payoff confirms that the included SQLite file can be read. Check correctness against the business rules and tests; the learner starter contains deliberate defects. The application binds only to `127.0.0.1:5080` and reads synthetic data from `src/Lighthouse.Api/Data/lighthouse.db`. No database-server setup is needed. Use a second terminal for tests and Git commands. Stop the API with **Ctrl+C** when finished.

## Repository map

| Path | Purpose |
| --- | --- |
| src/Lighthouse.Api/Services/LoanPayoffService.cs | Long calculation method for explanation, testing, and refactoring |
| src/Lighthouse.Api/Endpoints/LoanPayoffEndpoints.cs | Separate HTTP validation task for the cloud agent |
| src/Lighthouse.Api/Services/MemberService.cs | Member lookup and SQL construction used in the security review |
| src/Lighthouse.Api/Data/lighthouse.db | 500 members, 1,200 accounts, and 300 loans, all synthetic |
| docs/payoff-rules.md | Authoritative training business rules |
| docs/data-dictionary.md | Field meanings, fixture identities, and handling limits |
| tests/Lighthouse.Tests/LoanPayoffServiceTests.cs | Exercise 2 starting file |
| .github/copilot-instructions.md | Context and conventions for Copilot |
| .github/prompts/refactor-safely.prompt.md | Reusable refactoring prompt for a supported local chat session |
| .vscode/extensions.json | Recommended local editor extensions |

## Exercise checkpoints

1. Explain CalculatePayoff and document its actual assumptions. Compare the code with the rules rather than treating it as authoritative.
2. Generate independent rule-based tests, observe failures, correct the behavior, and run tests again.
3. Refactor without changing the corrected behavior. Inspect the diff and test output before a commit or pull request.

The learner starter begins with one smoke test and an empty payoff-test file. Its green baseline does not establish correct payoff logic or a secure API. Review the actual tests and executed count at every checkpoint.

## Chat context and the reusable prompt

Use Add Context or the available file/symbol picker to attach the service, its tests, and `docs/payoff-rules.md`. Use an interactive session that edits the local checkout, with the facilitator-approved permission level. The current VS Code interface may show Session Target, Agent, model, and Permissions; choose the actual explanation or editing behavior available to your account.

Run `.github/prompts/refactor-safely.prompt.md` from a Local session that supports prompt files. In a session without that command, open the file and paste its prompt body as an ordinary message, explicitly attaching the named files. Review proposed commands and edits. Approval prompts depend on the actual session and organization settings.

For the optional tone experiment, change and then restore only the instruction that begins **Explain for a product owner**. Keep the decimal, test, privacy, and permission instructions in place. Record what changes in the explanation; a visible difference is not guaranteed on every run.

## GitHub review and the independent cloud task

GitHub hosts the repository, pull requests, CI, and the separate asynchronous endpoint task. The learner's local editor session and the remote cloud-agent session are separate. Keep the facilitator's independent issue-to-PR demonstration and review its actual acceptance evidence.

`.github/workflows/ci.yml` runs the .NET restore and tests on a GitHub-hosted runner. `.github/workflows/copilot-setup-steps.yml` supplies the same SDK and commands for the **GitHub Actions-based** cloud-agent environment. That hook does not configure a managed cloud sandbox. If the organization uses a sandbox, rehearse the SDK, package access, and test execution through that environment's setup path. The supplied source does not provision GitHub resources or demonstrate that a live agent task has completed.

## Loan payoff rules

A quote is available for an active member and an open loan belonging to that member. The service accrues simple interest on the nonnegative principal for the elapsed calendar days since the interest as-of date, with a fixed 365-day denominator. A date before interest as-of accrues zero new interest. The grace period includes 15 days after the due date. A positive balance incurs one $25 fee only after the last grace day; a zero balance produces a zero quote. The service adds principal, unrounded interest, and the fee, then rounds the final total once to cents using AwayFromZero. The quote includes the day counts, grace-end date, and a calculation trace.

The initial implementation contradicted the approved rules in three places: a ten-day grace constant, an inclusive comparison that charged on the final grace day, and ToEven rounding. The worked solution first corrects those behaviors using independent tests, then extracts focused calculation methods, shares member validation, and reads `ConnectionStrings:Lighthouse` from configuration. The separate endpoint change produces consistent HTTP validation errors. These are fictional training rules; `docs/payoff-rules.md` remains the detailed authority.

## Supported training target

This edition targets .NET 10 with SDK 10.0.403, `net10.0`, and Microsoft.Data.Sqlite plus Microsoft.AspNetCore.Mvc.Testing 10.0.14. Preserve the committed lock files for repeatable restore. Before a future servicing update, align `global.json`, framework packages, and both workflow SDK pins, regenerate locks, then repeat the exercise checkpoints and device rehearsal.

Setup references: [official .NET 10 SDK downloads](https://dotnet.microsoft.com/en-us/download/dotnet/10.0), [C# in VS Code](https://code.visualstudio.com/docs/languages/csharp), [Copilot in VS Code](https://code.visualstudio.com/docs/setup/copilot), [VS Code chat sessions](https://code.visualstudio.com/docs/agents/run/chat-view), [prompt files](https://code.visualstudio.com/docs/agent-customization/prompt-files), and [Copilot on GitHub environments](https://docs.github.com/en/copilot/concepts/copilot-surfaces/copilot-on-github).
