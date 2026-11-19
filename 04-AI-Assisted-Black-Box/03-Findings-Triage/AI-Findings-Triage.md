# AI-Assisted Black-Box — Findings Triage

**Project:** FinalTouch  
**Assessment:** AI-Assisted Black-Box Security Assessment  
**Assessment Date:** 12 November 2026  
**Triage Date:** 15 November 2026  
**Status:** Manual validation completed for FT-01 to FT-08.

## 1. Triage Summary

This document records the manual review of findings reported during the AI-assisted black-box assessment.

The AI report identified eight findings. Each was reviewed against the application's implementation, development environment, and observed behavior.

The purpose of this triage is to distinguish actual security weaknesses from development-only configurations, expected application behavior, and informational observations.

| ID | Reported Finding | Final Disposition | Reason |
|---|---|---|---|
| FT-01 | Credentialed CORS with arbitrary origin reflection | Development configuration weakness | Permissive CORS policy is intentional for development; production is intended to use a strict origin allowlist. |
| FT-02 | Client-controlled cart price | No confirmed financial impact | Manipulated price affects cart data but does not carry through to payment. |
| FT-03 | Verbose exception disclosure | Not retained as a confirmed finding | Reported endpoints relate to test functionality or upcoming features. |
| FT-04 | Public Swagger/OpenAPI | Development-only configuration | Swagger is enabled inside the Development environment condition. |
| FT-05 | Vite development server and source-path disclosure | Informational / development-only | Exposure relates to the development server and local filesystem paths; no sensitive source retrieval was demonstrated. |
| FT-06 | Missing security headers | Development-only observation | Headers are intended to be configured for production; production configuration was not independently verified. |
| FT-07 | Unauthenticated cart CRUD with client-supplied IDs | No confirmed vulnerability | Accounts are isolated from each other's carts; cart manipulation does not persist into payment. |
| FT-08 | Kestrel server banner | Informational | Server technology fingerprinting only. |

## 2. Manual Triage Details

### FT-01 — Credentialed CORS with Arbitrary Origin Reflection

**AI severity:** High  
**Final disposition:** Development configuration weakness  
**Production applicability:** Not confirmed

The CORS behavior is caused by the current development configuration:

```csharp
policy.SetIsOriginAllowed(_ => true)
      .AllowAnyHeader()
      .AllowAnyMethod()
      .AllowCredentials();
```

This configuration allows arbitrary origins while also permitting credentialed cross-origin requests.

It is intentionally permissive for the current development phase. The production configuration is intended to restrict allowed origins to the trusted application/server domains.

**Security consideration:**

Allowing arbitrary origins together with credentials is an unsafe CORS configuration. If browser cookie policies permit the user's authentication cookies to accompany a cross-origin request, an untrusted website may be able to read responses from the API.

This also increases CSRF-related risk, although CORS and CSRF are separate security mechanisms and should not be treated as the same vulnerability.

**Final assessment:**

The behavior was observed, but it originates from the temporary development configuration. Production exposure has not been demonstrated.

**Recommendation:**

Use an explicit allowlist of trusted production origins. Do not reflect arbitrary origins or allow `null` origins with credentials enabled. Review CSRF protections independently for cookie-authenticated state-changing endpoints.

---

### FT-02 — Client-Controlled Cart Price

**AI severity:** High  
**Final disposition:** No confirmed financial impact  
**Security impact:** Limited to cart data manipulation based on manual validation

The AI assessment demonstrated that manipulated prices, including very low and negative values, could be submitted to the cart endpoint.

During manual testing, the manipulated price affected the cart display and the cart data stored in Redis. However, the manipulated value did not remain effective throughout the payment flow. When proceeding to payment, the application returned to the original product price.

Therefore, the test did not establish that an attacker could underpay, create a zero-value order, or manipulate the final payment amount.

**Final assessment:**

The cart endpoint accepts client-supplied prices and allows manipulated values to affect cart data. However, the payment flow restores the original product price, and no financial impact was confirmed.

This finding is not retained as a confirmed financial vulnerability.

**Recommendation:**

Continue resolving product prices server-side using authoritative product records. Validate prices and quantities during cart processing and again during checkout.

---

### FT-03 — Verbose Exception Disclosure

**AI severity:** Medium  
**Final disposition:** Not retained as a confirmed finding

The AI report identified verbose exception responses from endpoints associated with test functionality and upcoming application features.

The reviewed cases included:

- Test endpoints under `/api/Buggy`.
- Endpoints related to upcoming functionality, such as two-factor authentication.
- Error scenarios that do not demonstrate exposure through the application's normal completed functionality.

**Final assessment:**

The reported responses are associated with test or incomplete functionality. Based on the manual review, they are not retained as confirmed vulnerabilities affecting the application's normal functionality.

This disposition does not establish that all application endpoints handle exceptions securely.

**Recommendation:**

Ensure that production error handling returns generic responses and does not expose stack traces, internal paths, SQL details, or framework information.

---

### FT-04 — Public Swagger/OpenAPI Exposure

**AI severity:** Medium  
**Final disposition:** Development-only configuration  
**Production applicability:** Not confirmed

The application configures Swagger inside the Development environment condition:

```csharp
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

This means Swagger and Swagger UI are enabled when the application runs in Development.

**Final assessment:**

The observed exposure is consistent with the application's development configuration. It is not retained as a confirmed production vulnerability.

**Recommendation:**

Keep Swagger disabled in production unless there is a specific operational requirement. If required, restrict access through appropriate authentication and network controls.

---

### FT-05 — Vite Development Server and Source-Path Disclosure

**AI severity:** Medium  
**Final disposition:** Informational / development-only observation

The AI assessment identified:

- Vite development-server resources.
- Local filesystem paths in delivered frontend assets.
- Source-map references.
- Development-related project and dependency paths.

The observed paths refer to the local development environment. The assessment did not retrieve or decode source maps or demonstrate access to sensitive source files.

The application is currently being tested through a development server. A production deployment using a properly configured production build would not normally expose the same development-server resources.

**Final assessment:**

This is a development-environment exposure and information-disclosure observation. The disclosed local paths are not, by themselves, evidence of a critical source-code disclosure vulnerability.

It is therefore classified as Informational rather than a confirmed exploitable vulnerability.

**Recommendation:**

Deploy a production build instead of exposing the development server. Disable production source maps where appropriate and ensure development resources are not publicly accessible.

---

### FT-06 — Missing Security Headers

**AI severity:** Low  
**Final disposition:** Development-only observation  
**Production applicability:** Not independently verified

The AI report observed the absence of several security headers:

- `Strict-Transport-Security`
- `Content-Security-Policy`
- `X-Frame-Options`
- `X-Content-Type-Options`
- `Referrer-Policy`
- `Permissions-Policy`

The application is currently running in a development environment. The security headers are intended to be configured as part of the production deployment.

**Final assessment:**

The missing headers are considered a development-phase observation rather than a confirmed production vulnerability.

The production configuration has not been independently tested, so this assessment does not establish that the headers are correctly configured in the deployed production environment.

**Recommendation:**

Configure and verify appropriate security headers during production deployment. Ensure the CSP is compatible with the Angular application and configure HSTS only when the production HTTPS setup is ready.

---

### FT-07 — Unauthenticated Cart CRUD with Client-Supplied Cart IDs

**AI severity:** Low / Informational  
**Final disposition:** No confirmed vulnerability

The AI assessment observed that cart creation, retrieval, and deletion could be performed without authentication using client-supplied cart identifiers.

During manual testing, cart isolation was verified between separate accounts. Each account could access its own cart, but could not access another account's cart.

Additionally, cart manipulation affected the cart data stored in Redis, but the manipulated data did not persist into the payment process. The application returned to the original product data during payment.

**Final assessment:**

The unauthenticated cart operations are consistent with the application's guest/cart functionality. Manual testing did not demonstrate cross-account cart access, unauthorized cart modification, or unauthorized deletion of another account's cart.

The finding is therefore not retained as a confirmed IDOR/BOLA or broken access control vulnerability.

**Recommendation:**

Continue enforcing cart ownership and session isolation. Keep cart identifiers associated with the appropriate user or session and ensure that authoritative product data is revalidated during checkout.

---

### FT-08 — Kestrel Server Banner

**AI severity:** Informational  
**Final disposition:** Informational observation

The API responses disclose the server technology through:

```http
Server: Kestrel
```

This provides basic technology fingerprinting but does not demonstrate an exploitable vulnerability.

**Recommendation:**

Remove or minimize the server header where supported. This is an optional hardening measure rather than a priority vulnerability.

---

## 3. Final Triage Conclusion

The AI-assisted assessment provided useful coverage of the unauthenticated attack surface and identified several behaviors worth reviewing.

Manual validation showed that most observations were related to the development environment, expected application behavior, or informational technology disclosure.

The cart-price test demonstrated that manipulated prices could affect cart data stored in Redis. However, the payment flow restored the original product data, and no financial impact was confirmed.

Cart isolation testing also showed that separate accounts could not access each other's carts, so the AI-reported cart authorization concern was not confirmed.

The current triage distinguishes:

- Development configuration weaknesses.
- Application behavior with no confirmed security impact.
- Informational observations.
- Production hardening requirements.

No unverified AI finding has been promoted to the confirmed vulnerability register solely because it appeared in the automated report.

The original AI assessment report and its evidence files are retained separately to preserve the assessment record.

**Next phase:** White-box security assessment, including source-code review, SAST, software composition analysis (SCA), and targeted review of the relevant application security controls.