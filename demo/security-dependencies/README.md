# Security dependency fixture

**Facilitator demonstration only. This project is not part of Lighthouse runtime behavior.**

The manifest deliberately references Newtonsoft.Json 12.0.3 so GitHub can inventory a known vulnerable dependency. The API and learner test project must never reference this project or add it to their solution. No vulnerable JSON processing code or exploit payload is included. Do not deploy or publish the project.

| Item | Verified value |
| --- | --- |
| Ecosystem / package | NuGet / Newtonsoft.Json |
| Fixture target framework | net10.0, matching the migrated workshop baseline |
| Fixture version | 12.0.3 |
| GitHub advisory | GHSA-5crp-9r3c-p9vr |
| CVE | CVE-2024-21907 |
| Advisory severity | High |
| Affected range | Less than 13.0.1 |
| First fixed version in advisory | 13.0.1 |
| Workshop remediation target | 13.0.3, a fixed version, not a claim about the latest release |

The advisory describes resource exhaustion from very deeply nested JSON. This exercise examines dependency evidence and a version change; it never runs that condition.

Source checked November 1, 2026: [GitHub Advisory Database](https://github.com/advisories/GHSA-5crp-9r3c-p9vr). GitHub supports NuGet `.csproj` manifests in the [dependency graph](https://docs.github.com/en/code-security/reference/supply-chain-security/dependency-graph-supported-package-ecosystems).

## What a reviewer should verify

1. The repository dependency graph shows this exact package, version, and manifest path.
2. The Dependabot alert identifies the advisory above. A manifest alone is not evidence that a scan completed.
3. A prepared remediation PR changes only the intended version to 13.0.3 unless a reviewer approves additional necessary work.
4. The dependency diff removes the affected version and introduces no new known vulnerability at the configured policy threshold.
5. Main application tests still pass, and the isolated project remains outside the solution.

A remediation PR can pass dependency review. A red dependency-review check is expected only when the compared change introduces a dependency that violates the configured policy. Do not describe a successful remediation check as a failed prevention test.

Restoring this isolated fixture is unnecessary for manifest-based inventory. If an administrator rehearses restore or build explicitly, run only on synthetic workshop infrastructure. Do not suppress its vulnerability warning or disable NuGet audit globally to obtain a green demonstration.
