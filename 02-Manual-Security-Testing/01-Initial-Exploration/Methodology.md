# Initial Manual Security Exploration

## Objective

Perform an initial manual exploration of the FinalTouch web application to understand its functionality, identify exposed attack surfaces, and observe potentially insecure application behavior before conducting structured vulnerability testing.

## Testing Approach

The initial assessment began with external reconnaissance to identify exposed services, application technologies, and potentially accessible paths before interacting with the application's features.

After reconnaissance, the application was explored to understand its normal workflows and observe how the frontend communicates with the backend API.

Activities included:

1. Performing initial network and web content reconnaissance.
2. Exploring the application's available features and user workflows.
3. Identifying accessible application routes and API endpoints.
4. Observing HTTP requests and responses using Burp Suite.
5. Examining authentication, session handling, and access-control behavior using separate test accounts.
6. Observing how user input is processed and returned by the application.
7. Investigating cart, checkout, payment, and order workflows.
8. Identifying unexpected behavior and potential business-logic weaknesses.

## Tools

- Burp Suite Community Edition
- Web Browser
- Nmap
- Gobuster
- ffuf
- Wappalyzer

## Assessment Notes

This phase was exploratory and did not represent complete coverage of all application security risks.

Potential weaknesses identified during this phase were recorded for further investigation and validation.

The observations and results were subsequently used to guide a structured assessment based on the OWASP Top 10 (2025) framework.