Execute the approved FinalTouch black-box security assessment plan.

IMPORTANT: The approved plan and the `/audit` command are binding instructions. Do not reinterpret them as permission for broader testing.

### Authorized targets

- [https://192.168.1.20:4200](https://192.168.1.20:4200)
    
- [https://](https://5001)[192.168.1.20](https://192.168.1.20:4200):[5001](https://5001)
    

Only these exact origins are authorized.

### Execution scope

Execute Phase 0 through Phase 9 of the approved plan, limited to Tier A activities.

Perform the following:

1. Complete local tool inventory and preflight checks.
    
2. Check target reachability.
    
3. Perform normal TLS certificate validation.
    
4. Stop if either target has an untrusted certificate. Do not bypass validation.
    
5. Capture the passive HTTP baseline.
    
6. Discover same-origin routes through observed links and delivered assets.
    
7. Maintain a route ledger.
    
8. Inspect publicly accessible application behavior using approved GET requests.
    
9. Observe authentication and session behavior without submitting credentials.
    
10. Map the observable authorization surface without attempting privilege escalation.
    
11. Map input fields and business workflows without submitting state-changing requests.
    
12. Record error handling and information disclosure observations.
    
13. Package evidence and produce the assessment report.
    

### Strict restrictions

- Do not inspect source code, project files, source maps, IDE contents, or internal configuration.
    
- Do not invoke either source-review skill.
    
- Do not perform POST, PUT, PATCH, DELETE, or other state-changing requests.
    
- Do not submit login, registration, checkout, payment, or account-management forms.
    
- Do not perform IDOR, BOLA, BFLA, privilege escalation, SQL injection, XSS, SSRF, XXE, CSRF, or other exploit validation.
    
- Do not run ZAP, Burp Active Scan, Nuclei, sqlmap, fuzzers, directory brute force, or load testing.
    
- Do not follow external redirects or test external dependencies.
    
- Do not install dependencies or modify application configuration.
    
- Do not modify application or database state.
    
- Do not expose cookies, credentials, tokens, or personal data in reports.
    

### Approval gates

If an activity requires Tier B or Tier C approval, stop before performing it.

Record:

- The proposed test.
    
- Why it is relevant.
    
- The exact target and request method.
    
- Expected side effects.
    
- The evidence needed.
    
- A safe rollback or recovery plan, if applicable.
    

Wait for my explicit approval before proceeding.

### Reporting

Save raw evidence and reports only in the approved assessment case directory, outside the application repository.

Produce:

- Target and TLS inventory.
    
- Attack-surface map.
    
- Route ledger.
    
- Authentication and authorization observations.
    
- Business-logic workflow map.
    
- Potential security hypotheses.
    
- Confirmed findings only where reproducible evidence exists.
    
- Limitations and untested areas.
    
- Approval requests for subsequent testing.
    

Never claim that a test was executed if it was not.

Begin with preflight checks. If a hard-stop condition occurs, stop and report it instead of attempting to work around it.


