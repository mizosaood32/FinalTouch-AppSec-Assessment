# Manual Security Testing

## Overview

This section documents the manual security assessment of the FinalTouch web application.

The assessment began with exploratory testing to understand the application's functionality, API communication, authentication behavior, and business workflows.

Following the initial exploration, structured security testing was conducted using the OWASP Top 10 framework.

## Assessment Phases

### 1. Initial Exploration
Understanding application functionality, identifying attack surfaces, and observing unexpected application behavior.

### 2. OWASP-Based Assessment
Evaluating relevant application security risks through targeted manual testing.

### 3. Findings and Evidence
Documenting confirmed security weaknesses, reproduction steps, supporting evidence, impact, and remediation recommendations.

## Testing Tools

- Burp Suite Community Edition
- Browser Developer Tools
- Nmap
- Gobuster

## Documentation

- [Initial Exploration](01-Initial-Exploration/)
- [OWASP Top 10 Assessment](02-OWASP-Top-10-Assessment/)
- [Confirmed Findings](03-Confirmed-Findings/)
- [Assessment Summary](04-Assessment-Summary/)

## Scope and Limitations

Testing was conducted against the locally hosted FinalTouch application in an authorized environment.

The assessment was limited to the documented application components and testing activities. A category not tested should not be interpreted as secure.
