I am preparing an independent black-box security assessment of an authorized local application.

Use the existing `/audit` command definition as the primary assessment instructions.

Target arguments:  
[https://localhost:4200](https://localhost:4200) [https://localhost:5001](https://localhost:5001)

Your task in this phase is PLANNING ONLY.

STRICT RESTRICTIONS:

1. Do not read application source code, project files, IDE workspace contents, source maps, environment files, or internal configuration.
    
2. Do not modify application files, install packages, change configurations, or modify the database.
    
3. Do not perform active scans, fuzzing, exploitation, destructive requests, or state-changing tests.
    
4. Do not bypass TLS certificate validation.
    
5. Do not expand scope beyond the two supplied URLs and their confirmed in-scope routes.
    
6. Do not assume previously discovered vulnerabilities or use existing findings as a testing checklist.
    

PLANNING OBJECTIVES:

- Identify the exact authorized targets and their boundaries.
    
- Explain how reachability and TLS trust will be checked.
    
- Design a passive-first testing sequence.
    
- Identify relevant application attack surfaces from observable HTTP behavior.
    
- Include authentication, authorization, session handling, input handling, API behavior, business logic, and error handling.
    
- Identify tests that could modify application state or trigger payments.
    
- Separate safe passive checks from tests requiring my explicit approval.
    
- Define how evidence will be collected and how findings will be classified.
    

OUTPUT:

Produce a structured execution plan with:

1. Scope and assumptions.
    
2. Testing phases in execution order.
    
3. Proposed tools and commands, if applicable.
    
4. Expected side effects and approval requirements.
    
5. Evidence collection strategy.
    
6. Reporting format.
    
7. Tests that must not be performed without additional approval.
    

Do not execute the plan. Stop and wait for my approval.