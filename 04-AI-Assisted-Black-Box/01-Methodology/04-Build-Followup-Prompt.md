I am updating the previous assessment restrictions.

I explicitly authorize temporary TLS certificate verification bypass for this isolated local development assessment only.

### Authorized targets

- [https://192.168.1.20:4200](https://192.168.1.20:4200)
    
- [https://192.168.1.20:5001](https://192.168.1.20:5001)
    

### TLS exception

1. HTTPS encryption must remain enabled.
    
2. Certificate verification may be bypassed temporarily for these two exact origins only.
    
3. Do not use this exception for external domains, redirects, or third-party services.
    
4. Document the TLS verification limitation in the assessment report.
    
5. Do not interpret the certificate issue as an application vulnerability.
    

### Assessment authorization

I want an actual black-box web application security assessment, not just TLS checks and passive HTTP headers.

Proceed with:

1. Frontend and backend reconnaissance.
    
2. Technology and attack-surface discovery.
    
3. Route and API endpoint discovery.
    
4. Authentication and session assessment.
    
5. Authorization and access-control testing.
    
6. Input validation and injection testing.
    
7. XSS and SQL injection testing.
    
8. Business-logic testing.
    
9. Security misconfiguration testing.
    
10. Evidence collection and vulnerability reporting.
    

### Safety restrictions

- Only test the two authorized origins.
    
- Do not access production systems or external services.
    
- Do not perform denial-of-service, load testing, or destructive actions.
    
- Do not delete user data or modify application configuration.
    
- Do not trigger real payments or financial transactions.
    
- Use dedicated test accounts for authenticated testing.
    
- Do not expose credentials, cookies, or tokens in reports.
    
- Stop before any test that could cause irreversible changes or affect other users, and request approval.
    
- Distinguish confirmed vulnerabilities from potential findings and false positives.
    
- Record reproducible evidence for every confirmed finding.
    

Do not stop merely because TLS certificate verification fails; use the explicitly authorized temporary exception above.

Begin with both targets and produce an actual application security assessment.