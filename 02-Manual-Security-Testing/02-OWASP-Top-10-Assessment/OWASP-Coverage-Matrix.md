# OWASP Top 10 (2025) – Assessment Coverage Matrix

## Assessment Overview

The FinalTouch application was assessed against the OWASP Top 10 (2025) categories using manual testing, HTTP request manipulation, application behavior analysis, and dependency auditing.

The matrix summarizes the testing performed and its current results. Potential findings require further validation before being classified as confirmed vulnerabilities.

| OWASP Category | Testing Performed | Current Result |
|---|---|---|
| A01 – Broken Access Control | Cross-account cart and order access, admin endpoint access, role manipulation, registration parameter manipulation | No confirmed vulnerability |
| A02 – Security Misconfiguration | Swagger accessibility, CORS configuration review, development environment exposure | Needs further validation |
| A03 – Software Supply Chain Failures | Frontend `npm audit`, backend `dotnet list package --vulnerable` | Potential dependency vulnerabilities identified |
| A04 – Cryptographic Failures | Password hash comparison, authentication cookie inspection, HTTP traffic review | No confirmed vulnerability |
| A05 – Injection | Initial XSS and SQL injection testing, URL and search input testing | Partial testing; no confirmed vulnerability |
| A06 – Insecure Design | Cart, checkout, product selection, and payment workflow manipulation | Confirmed business-logic issue |
| A07 – Authentication Failures | Invalid login attempts, session behavior, logout behavior, authentication testing | Potential missing rate limiting |
| A08 – Software or Data Integrity Failures | Product ID manipulation, order record verification, Stripe test payment verification | Confirmed product/price integrity issue |
| A09 – Security Logging and Alerting Failures | Review of application and database event visibility | Partial testing; insufficient evidence |
| A10 – Mishandling of Exceptional Conditions | Malformed URL testing, invalid identifiers, forged cookies, missing request fields | Development error disclosure observed |

## Notes

- A confirmed issue may be relevant to more than one OWASP category.
- No confirmed vulnerability does not mean that the category is completely secure.
- Categories marked as partial or requiring validation were not exhaustively assessed.
- Dependency audit results require individual package and advisory verification.
- Development-only behavior must not automatically be treated as production exposure.

## Overall Assessment

The assessment identified a confirmed checkout product/price integrity weakness, potential frontend dependency vulnerabilities, and several configuration and error-handling observations requiring further validation.

Other tested areas, particularly cross-account access and role manipulation, did not produce confirmed vulnerabilities during the assessment.