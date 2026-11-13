# OWASP ZAP – Automated Security Testing Methodology

## 1. Objective

OWASP ZAP was used to perform automated security testing against the locally hosted FinalTouch application.

The objective was to identify potential security weaknesses, discover application routes, inspect HTTP traffic, and evaluate the behavior of the frontend and backend through passive and active scanning.

## 2. Testing Environment

| Component | Address |
|---|---|
| Angular Frontend | `https://192.168.1.20:4200` |
| ASP.NET Core API | `https://192.168.1.20:5001` |
| ZAP Context | `finaltouch-test` |

Both application URLs were added to the ZAP context.

The browser was configured to route application traffic through ZAP for inspection.

## 3. Testing Activities

### 3.1 Passive Scanning

Passive scanning was performed while browsing the application through the configured ZAP browser.

ZAP inspected the observed HTTP requests and responses and generated alerts relating to security headers, CORS, cookies, information disclosure, and other configuration-related observations.

Passive scanning did not intentionally modify application requests to exploit vulnerabilities.

### 3.2 Spidering and Route Discovery

The ZAP Spider was used to discover application routes, resources, and links.

The discovered routes were compared with the application's known frontend and API endpoints.

This activity provided additional visibility into the application's exposed attack surface.

### 3.3 OpenAPI Import

The application's Swagger/OpenAPI definition was imported into ZAP using:

`/swagger/swagger.json`

This allowed ZAP to discover API endpoints and use the imported API definition during automated testing.

The API definition was available because the application was running in its development configuration.

This approach provided additional endpoint coverage but would not necessarily be available during a black-box assessment.

### 3.4 Active Scanning

ZAP Active Scan was performed against the frontend and backend targets.

The scan attempted automated attack payloads against discovered endpoints and parameters to identify potential vulnerabilities.

The testing included:

- Frontend target: port `4200`.
- Backend target: port `5001`.

Active scanning was performed against the locally hosted test application.

### 3.5 Manual Validation

Interesting alerts were investigated manually by reviewing the associated requests and responses, reproducing the reported behavior, and checking application behavior and database records where relevant.

Alerts were classified according to the available evidence rather than accepted automatically as confirmed vulnerabilities.

## 4. Scope and Limitations

- Testing was performed against a local development deployment.
- The OpenAPI definition was imported to improve API endpoint coverage.
- Development-specific behavior and configuration may differ from production.
- Automated scanner alerts may include false positives or configuration observations.
- ZAP does not replace manual business-logic testing.
- The checkout product/price integrity issue identified during manual testing was not independently detected by the ZAP scans.

## 5. Evidence

Screenshots, scan results, and relevant request/response evidence are stored in the ZAP Evidence directory.
