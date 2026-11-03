# Lighthouse security demonstration repository

This is the facilitator copy of the fictional Lighthouse Member Services lab. All member data is synthetic. Keep the repository private to the workshop unless its owner has explicitly chosen another visibility.

The source intentionally contains the member-lookup SQL construction used for source review. The `demo/security-dependencies/` manifest intentionally references one affected package version for dependency review. Neither is a production example. The application should run on the loopback interface with synthetic data only.

## Delivery status

The source files and preparation procedure are supplied. This package does not enable GitHub products, create alerts, create a campaign, open pull requests, or capture screenshots. Those outcomes require the delivery organization and the administrator's rehearsal.

Use the separate Advanced Security demo script and preparation runbook. Record the actual alert URLs, commit SHAs, check runs, enabled settings, and rehearsal time before calling the environment ready.

## Isolation rules

- Never copy `demo/security-dependencies/` into the learner template.
- Never add its project to the main solution or reference it from the API or tests.
- Never use a real credential for the push-protection demonstration.
- Never treat a fake provider-shaped key as a guaranteed detected secret.
- The training marker generator only writes local harmless fixtures. It does not commit, push, contact a service, or grant access.
- Use CodeQL default setup for the recommended route. Do not also add an active advanced CodeQL workflow or upload a second CodeQL analysis configuration.

The security features, screens, and permission names depend on current product rollout and organization policies. Inspect the actual delivery account during rehearsal.
