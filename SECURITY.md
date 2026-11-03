# Security boundaries for this training code

This worked training implementation parameterizes the SQL search demonstrated in the starter. It still has no authentication layer and is for isolated, synthetic-data training. Keep the local API bound to 127.0.0.1. Do not expose it on a public endpoint or use it with real member data.

The configured SQLite connection string is a local file path, not a secret and not evidence that provider secret scanning should alert. Credential-shaped fixtures and intentionally vulnerable package versions belong only in the separate facilitator security repository.

Use this project to practice inspecting data flow, proposing a parameterized fix, testing it, and reviewing a pull request. A local code review or a generated patch is not proof that GitHub CodeQL, push protection, Autofix, or a security campaign executed. Those must be rehearsed in the delivery organization.
