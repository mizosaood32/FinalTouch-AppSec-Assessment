
---
description: Autonomous black-box security assessment of an authorized local target
---

Perform an independent black-box security assessment.

Target arguments: $ARGUMENTS

Interpret the arguments as the authorized target URLs. For example:
https://localhost:4200 https://localhost:5001

Rules:
- Treat only the explicitly supplied target URLs and their confirmed in-scope routes as authorized.
- Do not read application source code, project files, IDE workspace contents, source maps, or internal configuration.
- Discover the application's attack surface from its observable behavior.
- Identify which services are reachable before testing them.
- If a target is unreachable or its TLS certificate is untrusted, report the issue and ask before changing configuration or bypassing certificate validation.
- Start with passive and low-impact inspection.
- Ask before active scans, destructive requests, persistent state changes, or tests that could trigger payments.
- Do not silently expand scope to other hosts, ports, or discovered external services.
- Report evidence, reproduction steps, impact, confidence, and limitations.
- Do not assume vulnerabilities in advance or stop after checking generic security headers.





