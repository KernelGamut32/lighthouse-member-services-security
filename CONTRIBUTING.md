# Contributing to the training repository

Use a personal branch such as `refactor/payoff-service`. Keep the exercise change small enough to explain in a review. The repository is a local training fixture and must not be deployed as a production service.

## Code and test conventions

- C# uses four spaces, PascalCase public types/members, and descriptive local names.
- Monetary values and annual rates use decimal and m suffixes. Dates use DateOnly and explicit fixtures.
- Tests use xUnit and the name Method_Scenario_ExpectedResult. Arrange independent inputs and expected values.
- Preserve signatures during the refactor. A separate bug-fix commit can change behavior to match the rules.
- Keep request validation at the HTTP boundary and business invariants in the domain service.
- Use parameterized SQL. The starter contains the unsafe search used in the security demonstration; this worked implementation parameterizes it.
- Avoid logging members' personal or financial fields, even though this dataset is synthetic.

## Before opening a pull request

Run `dotnet test Lighthouse.sln`. Read the diff. Explain the problem, what changed, actual tests, behavior changes, and remaining limitations. Review any public-code match or license concern raised by Copilot according to your organization's policy. Copilot review is advisory; a person remains responsible for the merge.

Do not suppress failing tests, bypass branch protections, or merge directly to main during the lab. Do not include real credentials or member information.
