# OWASP ZAP – Automated Security Testing Results

## 1. Overview

OWASP ZAP was used to perform passive scanning, application spidering, OpenAPI-assisted endpoint discovery, and active scanning against the locally hosted FinalTouch application.

Both the Angular frontend and ASP.NET Core API were included in the testing scope.

The scanner generated multiple alerts. Selected alerts were manually investigated to distinguish confirmed behavior from false positives and unverified security concerns.

## 2. Passive Scan Results

The passive scan generated alerts across several categories.

### Medium-Risk Alerts

- Application error disclosure.
- Content Security Policy directives and fallback configuration.
- Content Security Policy wildcard usage.
- `unsafe-eval` in script policy.
- `unsafe-inline` in style policy.
- Missing Content Security Policy header.
- Cross-origin resource sharing misconfiguration.
- Missing anti-clickjacking protection.
- Missing Subresource Integrity attributes.

### Low-Risk Alerts

- Application error disclosure.
- Potential sensitive information disclosure through redirects.
- Content Security Policy notices.
- Cookie SameSite attribute observations.
- Cross-domain JavaScript source inclusion.
- Server version disclosure.
- Missing HTTP Strict Transport Security header.
- Timestamp disclosure.
- Missing X-Content-Type-Options header.

### Informational Alerts

- Authentication request identified.
- Content Security Policy report-only header detected.
- Missing or empty Content-Type observations.
- Local storage information disclosure alert.
- Potential sensitive information in URLs.
- Suspicious comments.
- Modern web application identification.
- Cache-control and cached-response observations.
- Session management response identified.

### Validation Notes

Several alerts were related to the local development configuration.

In particular, the CORS alert was influenced by a temporary permissive CORS policy introduced during lab setup to allow connectivity from the testing VM.

The policy was not treated as a confirmed vulnerability in the application's original configuration.

Other alerts required context-specific review. For example:

- Development error disclosures involving local filesystem paths require production verification.
- Swagger and development resources should be evaluated according to whether they are exposed in production.
- Missing security headers are configuration observations that require review, rather than automatically exploitable vulnerabilities.
- The redirect involving the images directory did not demonstrate sensitive information disclosure during validation.
- The cross-domain JavaScript alert was associated with the hCaptcha integration.

## 3. Spider Results

The ZAP Spider discovered many of the application's frontend routes, API-related resources, and static assets.

The discovered routes were largely consistent with the application's known structure.

The Spider also identified additional resource requests and file references.

This activity provided useful attack-surface visibility, although the application's known routes were already available from previous manual testing.

## 4. OpenAPI Import

The Swagger/OpenAPI definition was imported through:

`/swagger/swagger.json`

This allowed ZAP to identify API endpoints that might not have been discovered through ordinary browsing or spidering.

The imported API definition improved the coverage of the active scan.

This was a Gray-box-assisted testing approach rather than a purely black-box discovery process.

## 5. Active Scan Results and Manual Validation

### 5.1 Swagger UI Dependency Alerts

**Classification:** Requires dependency and version validation.

The active scan identified a potential vulnerability involving Swagger UI-related JavaScript resources.

The application used:

- Swashbuckle.AspNetCore: `8.1.1`.
- Swagger UI: reported as bundled version `5.x`.
- Reported advisories: CVE-2026-66010 and CVE-2026-75838.
- Related DOMPurify advisory: GHSA-c2j3-45gr-mqc4.

The DOMPurify advisory requires verification of the actual installed DOMPurify version and whether the affected configuration and behavior are relevant to the application.

Swagger was enabled only under the development environment condition in the application configuration:

`if (app.Environment.IsDevelopment())`

Therefore, exposure of the Swagger UI in production should be prevented by the intended configuration.

The dependency alert still requires independent version verification and should not be treated as a confirmed application vulnerability solely because Swagger UI is bundled with Swashbuckle.

### 5.2 User-Agent Fuzzer – `/images`

**Classification:** Informational; no vulnerability established.

ZAP performed User-Agent fuzzing against the images endpoint.

The test changed the User-Agent request header to simulate different clients and browsers and observed the server's responses.

No security vulnerability was established from this test.

### 5.3 Reflected XSS Alert – Payment-Related Testing

**Classification:** Not confirmed; likely false positive based on validation.

ZAP reported a possible reflected XSS issue during payment-related testing.

During investigation, a script-looking payload was submitted in a JSON request body. The payload was returned or stored as text, but JavaScript execution was not demonstrated.

Additional attempts did not establish a successful XSS condition.

The alert was therefore not promoted to a confirmed XSS finding.

The exact request and parameter associated with the alert should be retained as evidence.

### 5.4 Format String Error – `/api/product`

**Classification:** Not confirmed; requires further investigation.

ZAP reported a possible format-string-related error involving the product API.

The reported behavior was investigated, but a confirmed format-string vulnerability was not established.

A format-string issue requires evidence that user-controlled input is interpreted as a format expression or causes a security-relevant effect. An error response alone is insufficient.

The associated request, parameter, and server response should be retained for further investigation.

### 5.5 SQL Injection Alert – `/api/cart`

**Classification:** Not confirmed; likely false positive based on validation.

ZAP reported a possible SQL injection issue involving the cart API.

During manual investigation, the tested input appeared to modify the displayed cart item name, while the product identifier and price remained unchanged.

The final order record and product table were also inspected. The tests did not demonstrate unauthorized changes to product records or a successful SQL injection condition.

The cart implementation was observed to use Redis for cart-related operations. This provided additional context, but the exact backend data flow was not independently established by this test.

No confirmed SQL injection vulnerability was demonstrated.

## 6. Confirmed Authorization Finding – Product Management API

<badge color="danger">Confirmed – F-02</badge>

**Finding:** Broken Access Control – Missing Authorization on Product Management Endpoints

**OWASP Mapping:** A01:2025 – Broken Access Control

During investigation of the product API, it was confirmed that product-management endpoints could be accessed by users without the required administrative authorization.

The affected operations included:

- `POST /api/products` – Product creation.
- `PUT /api/products` – Product modification.
- `DELETE /api/products` – Product deletion.

The requests could be performed using a regular user session and were also accessible without an authentication cookie.

The application did not adequately enforce the required authorization checks for these administrative operations.

### Security Impact

An unauthorized user could potentially:

- Create products in the application catalog.
- Modify existing product information.
- Delete products from the catalog.

This affects the integrity and availability of product data and could allow unauthorized manipulation of the application's catalog.

### Assessment

This is a Broken Access Control issue because the server does not properly restrict administrative product-management operations to authorized administrators.

The issue is separate from the format-string alert described in Section 5.4.

**Finding reference:** `F-02-Product-Management-Broken-Access-Control.md`

Supporting request/response evidence should be retained in the ZAP evidence directory, including the tested authentication state and the resulting application response.

## 7. Comparison with Manual Testing

The manual assessment identified a checkout product/price integrity weakness involving product identifier manipulation.

ZAP did not independently identify this business-logic issue during the automated scans.

The authorization issue involving product-management endpoints was identified during investigation of the product API alerts and confirmed through manual authorization testing.

This demonstrates a limitation of automated scanning: the scanner may identify common technical weaknesses, but application-specific authorization requirements and business rules still require manual verification.

## 8. Summary and Current Classification

| Alert / Observation | Current Classification |
|---|---|
| Swagger UI / DOMPurify dependency | Requires version and applicability validation |
| User-Agent fuzzing | Informational |
| Reflected XSS | Not confirmed |
| Format string error | Not confirmed |
| SQL injection | Not confirmed |
| Product Management API authorization | Confirmed – F-02 |
| CORS misconfiguration | Influenced by temporary lab configuration |
| Development error disclosure | Requires production verification |
| Missing security headers | Configuration observations requiring review |

## 9. Conclusion

OWASP ZAP provided useful visibility into the application's HTTP behavior, security headers, endpoint structure, and potential technical weaknesses.

Manual validation confirmed a Broken Access Control vulnerability affecting product-management operations. Regular users and unauthenticated guests could access product creation, modification, and deletion endpoints without the required administrative authorization.

This issue is documented separately as F-02.

The manual assessment also identified the checkout product/price integrity weakness, documented as F-01, which ZAP did not independently detect.

Several other scanner alerts were investigated but were not confirmed as vulnerabilities.

The ZAP results will be correlated with the manual and AI-assisted testing phases before the final consolidated findings are produced.

## 10. Evidence

Supporting screenshots and request/response evidence are stored under:

`03-Automated-Security-Testing/ZAP/Evidence/`