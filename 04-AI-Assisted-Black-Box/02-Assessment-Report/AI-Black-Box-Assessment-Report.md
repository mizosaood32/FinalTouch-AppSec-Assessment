# FinalTouch — Active Black-Box Security Assessment

- **Status:** Active phase complete for the unauthenticated surface; authenticated phase blocked pending test accounts.
- **Date:** 2026-11-12
- **Case:** `finaltouch-audit-192.168.1.20-20261112-024058`
- **Authorized origins:**
  - `https://192.168.1.20:4200`
  - `https://192.168.1.20:5001`
- **Method:** Black-box web application assessment; no source code, repository, source maps, environment files, or internal configuration inspected.
- **TLS note:** Certificate verification was temporarily bypassed only for the two exact authorized origins, as explicitly authorized. HTTPS encryption remained enabled. Both certificates are development/self-signed certificates that are not valid for `192.168.1.20`; this is an environment/deployment issue, not an application vulnerability.

## 1. Executive Summary

The tested application appears to be a development/training deployment. It exposes an Angular frontend on port `4200` through a Vite development server and an ASP.NET Core/Kestrel API on port `5001`.

The most significant confirmed issues are:

1. **Client-controlled cart pricing.** `POST /api/Cart` accepts arbitrary item prices, including `0.01` and `-1000`, and persists them without validating against the product catalog or enforcing non-negative values.
2. **Credentialed CORS with arbitrary origin reflection.** The API reflects arbitrary `Origin` values, including `https://example.invalid` and `null`, while returning `Access-Control-Allow-Credentials: true`.
3. **Verbose error disclosure.** Unhandled exceptions return stack traces, local filesystem paths, SQL Server error details, and framework internals.
4. **Swagger/OpenAPI exposure.** The full API schema is publicly reachable without authentication.
5. **Vite development-server exposure.** The frontend exposes `/@vite/client`, 277 `/@fs/D:/...` references, and source-map references in delivered assets.

SQL injection was not confirmed in the unauthenticated parameters tested. Reflected XSS was not observed in the API error responses reviewed, but browser-based XSS testing was not performed. Authenticated authorization, CSRF, IDOR/BOLA, stored XSS, and checkout/order-price impact could not be validated because no dedicated test accounts were supplied.

**Overall risk:** High for the tested environment as a configuration and business-logic assessment. Several findings may be intentional in a training project; production applicability must be confirmed.

## 2. Scope and Rules of Engagement

### In scope
- `https://192.168.1.20:4200` — frontend and delivered static assets.
- `https://192.168.1.20:5001` — API, Swagger, authentication endpoints, and error handling.

### Explicitly out of scope
- Any other host, IP, port, subdomain, redirect target, external service, or production system.
- Source code, repository contents, IDE workspace, source maps, environment files, and internal configuration.
- Real payments, financial transactions, denial-of-service, load testing, destructive actions, deletion of user data, and configuration changes.

### Safety controls applied
- Requests were limited to the two authorized origins.
- TLS verification bypass was used only with `curl -k` against those exact origins.
- The only state-changing tests were isolated, reversible test-cart operations, followed by deletion of each test cart.
- No checkout or payment endpoint was invoked.
- No credentials, cookies, or tokens were captured or included in this report.
- Source-map and `/@fs/...` content was not fetched or decoded.

## 3. Risk Summary

| ID | Finding | Severity | Confidence | Status |
|---|---|---|---|---|
| FT-01 | Credentialed CORS reflects arbitrary origins | High | High | Confirmed |
| FT-02 | Client-controlled cart item price, including negative price | High | High for cart; Medium for checkout impact | Confirmed |
| FT-03 | Verbose exceptions disclose stack traces, SQL details, and filesystem paths | Medium | High | Confirmed |
| FT-04 | Swagger/OpenAPI schema publicly exposed | Medium | High | Confirmed |
| FT-05 | Vite dev server and source path/source-map disclosure | Medium | High for path/source-map references; source retrieval unconfirmed | Confirmed / potential |
| FT-06 | Missing security headers | Low | High | Confirmed |
| FT-07 | Unauthenticated cart CRUD with client-supplied cart IDs | Low / Informational | High | Confirmed as behavior |
| FT-08 | Kestrel server banner | Informational | High | Confirmed |

## 4. Confirmed Findings

### FT-01 — Credentialed CORS reflects arbitrary origins

**Severity:** High (configuration weakness)
**Confidence:** High
**Affected endpoints tested:**
- `/api/Account/auth-status`
- `/api/ProductQueries`
- `/api/Payment/delivery-methods`

**Observed behavior**

Requests with an arbitrary `Origin` header were answered with that exact origin reflected:

```http
Origin: https://example.invalid

HTTP/1.1 200 OK
Access-Control-Allow-Credentials: true
Access-Control-Allow-Origin: https://example.invalid
Vary: Origin
```

The same behavior occurred with:

```http
Origin: null

Access-Control-Allow-Credentials: true
Access-Control-Allow-Origin: null
```

The legitimate frontend origin `https://192.168.1.20:4200` was also reflected.

**Impact**

A malicious website can make credentialed cross-origin requests to the API and read responses if the browser sends the user's authentication cookies. This can expose authenticated data and enable cross-site actions that rely on the victim's session.

Exploitability depends on the session cookie's `SameSite`, `Secure`, and domain attributes, which could not be observed without a successful login and dedicated test account. The CORS policy itself is confirmed to be unsafe.

**Remediation**

- Replace origin reflection with a strict allowlist of exact trusted frontend origins.
- Never combine `Access-Control-Allow-Credentials: true` with `AllowAnyOrigin` or reflected origins.
- Do not allow `null` as an authorized origin.
- Use `SameSite=Lax` or `SameSite=Strict` cookies where compatible; set `Secure` and `HttpOnly`.
- Add CSRF defenses for cookie-authenticated state-changing requests.

**Evidence**

- `05-surface\cors\cors-log.txt`
- `05-surface\cors\https___example_invalid_*.headers.txt`
- `05-surface\cors\null_*.headers.txt`

---

### FT-02 — Client-controlled cart item price and negative price accepted

**Severity:** High (business logic)
**Confidence:** High for cart price trust; Medium for checkout/financial impact
**Affected endpoint:** `POST /api/Cart`, `GET /api/Cart`, `DELETE /api/Cart`

**Observed behavior**

A unique test cart was created with a valid product ID but a manipulated price:

```json
{
  "id": "<unique-test-cart-guid>",
  "items": [
    {
      "productId": 1965,
      "productName": "Anti-Mold Paint",
      "price": 0.01,
      "quantity": 1,
      "pictureUrl": "images/productss/paint7.jpg",
      "brand": "FinalTouch",
      "type": "Paint"
    }
  ],
  "deliveryMethodId": null,
  "clientSecret": null,
  "paymentIntentId": null
}
```

`POST /api/Cart` returned `200` and echoed `"price":0.01`. A subsequent `GET /api/Cart?id=<test-cart>` returned the same manipulated price, confirming persistence.

A second isolated test cart used a negative price:

```json
"price": -1000
```

`POST /api/Cart` returned `200`, and `GET /api/Cart` returned the persisted negative value. Both test carts were deleted with `DELETE /api/Cart?id=<test-cart>` returning `200`.

**Impact**

The server trusts client-supplied cart item prices instead of resolving the authoritative product price from its own data store. If checkout or order creation calculates totals from these cart values, an attacker could underpay, create zero-value orders, or potentially create negative totals. Checkout and payment were not tested, so the final order-total impact is not confirmed.

**Remediation**

- Ignore client-supplied prices; resolve price server-side by product ID.
- Compute all totals server-side at cart validation and checkout.
- Reject negative prices and non-positive quantities.
- Revalidate price, stock, and quantity at order creation.
- Bind carts to an authenticated user or signed session and authorize all cart operations.

**Evidence**

- `09-business\cart-price-test\cart-price-test-log.txt`
- `09-business\cart-price-test\create.body.bin`
- `09-business\cart-price-test\get.body.bin`
- `09-business\cart-price-test\negative-create.body.bin`
- `09-business\cart-price-test\negative-get.body.bin`
- `09-business\cart-price-test\delete.headers.txt`
- `09-business\cart-price-test\negative-delete.headers.txt`

---

### FT-03 — Verbose exceptions disclose stack traces, SQL details, and filesystem paths

**Severity:** Medium (High if deployed to production)
**Confidence:** High
**Affected areas:** Global exception handling; pagination validation; login body parsing; intentionally buggy endpoints.

**Observed behavior**

`GET /api/Buggy/internalerror` returned `500` with a full stack trace and an absolute server path:

```json
{
  "statusCode": 500,
  "message": "This is a test exception",
  "details": "   at API.Controllers.BuggyController.GetInternalError() in D:\\ ..."
}
```

A malformed JSON body sent to `POST /api/login?useCookies=true` returned `500` with a stack trace and another local path reference.

`GET /api/ProductQueries?pageSize=-1` returned `500` with:

```text
The number of rows provided for a FETCH clause must be greater then zero.
```

`GET /api/ProductQueries?pageIndex=0` and `?pageIndex=-1` returned `500` with:

```text
The offset specified in a OFFSET clause may not be negative.
```

The responses included ASP.NET Core and `Microsoft.Data.SqlClient` stack frames.

**Impact**

Error responses reveal internal file paths, controller/class names, framework versions, SQL Server behavior, and application structure. This assists targeted attacks and can expose sensitive implementation details.

**Remediation**

- Return a generic error response with a correlation/trace ID in production.
- Log stack traces only server-side.
- Validate pagination parameters before they reach the data layer.
- Disable detailed exception pages and developer exception middleware in production.
- Return `400` for malformed request bodies and invalid parameter ranges.

**Evidence**

- `05-surface\unauth-get\api-16.body.bin`
- `06-auth\login-invalid.body.bin`
- `08-input\validation-productqueries\validation-log.txt`
- `08-input\validation-productqueries\02-pageSize-negative.body.bin`
- `08-input\validation-productqueries\03-pageIndex-zero.body.bin`
- `08-input\validation-productqueries\04-pageIndex-negative.body.bin`

---

### FT-04 — Swagger/OpenAPI schema publicly exposed

**Severity:** Medium
**Confidence:** High
**Affected endpoints:**
- `/swagger` → `301` to `/swagger/index.html`
- `/swagger/index.html` → `200`
- `/swagger/v1/swagger.json` → `200`, 53,378 bytes

**Observed behavior**

The Swagger UI and OpenAPI document were reachable without authentication. The schema exposed all API paths, HTTP methods, parameter names, request/response models, and authentication-related endpoints, including admin, order, payment, cart, and account operations.

**Impact**

The schema gives an attacker a complete map of the API and its data models, reducing reconnaissance effort. It also exposes administrative and payment-related route names that would otherwise require discovery.

**Remediation**

- Disable Swagger in production, or restrict it to an internal/admin-only network and require authentication.
- Do not publish internal or administrative endpoints in a public schema.
- Review the OpenAPI document for sensitive examples, descriptions, and server information.

**Evidence**

- `05-surface\docs\docs-request-log.txt`
- `05-surface\docs\doc-02.body.bin`
- `05-surface\docs\doc-03.body.bin`
- `05-surface\docs\swagger-summary.txt`
- `05-surface\docs\swagger-parameters.txt`
- `05-surface\docs\swagger-schemas.txt`

---

### FT-05 — Vite development server and source path/source-map disclosure

**Severity:** Medium (potential High for source disclosure)
**Confidence:** High for path/source-map references; source retrieval not tested
**Affected origin:** `https://192.168.1.20:4200`

**Observed behavior**

- `/@vite/client` returned `200` with a Vite client bundle.
- `main.js` (884,544 bytes) contained **277** `@fs/D:/...` references, for example:

```text
@fs/D:/
```

- `main.js` contained **29** `sourceMappingURL` references.
- `styles.css` ended with:

```css
/*# sourceMappingURL=styles.css.map */
```

- The frontend bundle exposed API and hub base URLs:
  - `https://192.168.1.20:5001/api/`
  - `https://192.168.1.20:5001/hub/notifications`
- The bundle referenced `cart_id` in client-side storage.

**Impact**

The Vite development server is exposed on the network, and delivered assets disclose the developer's absolute filesystem paths, project structure, dependency cache layout, and source-map references. If the dev server serves `/@fs/...` or `.map` files, source code could be retrievable. That retrieval was not tested because source-map/source inspection remains prohibited.

**Remediation**

- Serve a production build, not the Vite development server.
- Bind the dev server to `localhost` only and never expose it on a network interface.
- Disable source maps in production builds.
- Remove absolute path leakage and development cache references from delivered assets.
- Restrict frontend and API access to trusted networks/users.

**Evidence**

- `05-surface\asset-fetch.txt`
- `05-surface\assets\main.js`
- `05-surface\assets\styles.css`
- `05-surface\assets\vite-client.js`
- `05-surface\assets\vite-client.headers.txt`

---

### FT-06 — Missing security headers

**Severity:** Low
**Confidence:** High
**Affected origins:** `4200` and `5001`

**Observed behavior**

Responses from both origins lacked:

- `Strict-Transport-Security`
- `Content-Security-Policy`
- `X-Frame-Options`
- `X-Content-Type-Options`
- `Referrer-Policy`
- `Permissions-Policy`

The frontend baseline returned `Cache-Control: no-cache` and `Vary: Origin`. The API returned `Server: Kestrel` and no security headers.

**Impact**

The absence of these headers weakens defense-in-depth against clickjacking, MIME sniffing, referrer leakage, and some cross-site scripting/data-injection scenarios. HSTS absence allows potential protocol downgrade on first visit if HTTP is reachable.

**Remediation**

- Add HSTS on HTTPS responses.
- Add a restrictive Content Security Policy appropriate for the Angular application.
- Add `X-Frame-Options: DENY` or CSP `frame-ancestors 'none'`.
- Add `X-Content-Type-Options: nosniff`.
- Add `Referrer-Policy: no-referrer` or a suitable strict policy.
- Add a minimal `Permissions-Policy`.

**Evidence**

- `04-http\baseline-4200.headers.txt`
- `04-http\baseline-5001.headers.txt`
- `05-surface\cors\cors-log.txt`
- `05-surface\unauth-get\request-log.txt`

---

### FT-07 — Unauthenticated cart CRUD with client-supplied cart IDs

**Severity:** Low / Informational
**Confidence:** High
**Affected endpoint:** `/api/Cart`

**Observed behavior**

- `GET /api/Cart?id=<arbitrary-guid>` returned `200` with an empty cart and echoed the supplied ID.
- `POST /api/Cart` created a cart with a client-supplied ID.
- `DELETE /api/Cart?id=<test-cart>` returned `200` without authentication.

All operations were performed only against isolated test carts created for the test, which were deleted afterward.

**Impact**

Cart operations do not appear to require authentication or ownership checks. This may be intentional guest-cart behavior. However, if a cart ID is disclosed, another party could read, modify, or delete that cart. GUID identifiers reduce guessability. Combined with FT-02, this increases the impact of cart tampering.

**Remediation**

- Bind carts to a server-side session or authenticated user.
- Generate cart identifiers server-side and treat them as opaque.
- Authorize read, update, and delete operations against the owning session/user.
- Rate-limit cart creation and mutation.
- Do not trust client-supplied cart contents or identifiers.

**Evidence**

- `05-surface\unauth-get\request-log.txt`
- `05-surface\unauth-get\buggy-extra-log.txt`
- `08-input\sqli-cart-id\cart-id-log.txt`
- `09-business\cart-price-test\cart-price-test-log.txt`

---

### FT-08 — Kestrel server banner

**Severity:** Informational
**Confidence:** High

The API returns `Server: Kestrel` in responses. This reveals the web server technology and can assist fingerprinting. It is low risk by itself.

**Remediation:** Remove or minimize the `Server` header where supported.

**Evidence:** `04-http\baseline-5001.headers.txt`

## 5. Potential Findings and Untested Hypotheses

These are not confirmed vulnerabilities. They require authenticated test accounts or additional approval.

1. **CSRF via cookie authentication and permissive CORS.** The frontend uses cookie authentication (`useCookies=true`) and sends credentials with API requests. No anti-CSRF token was observed. Combined with FT-01, CSRF may be possible depending on cookie `SameSite` attributes and request content types. Requires successful login and cookie inspection.
2. **IDOR/BOLA on orders and carts.** `/api/orders/{id}`, `/api/admin/orders/{id}`, and cart endpoints are ID-based. Cart IDs are GUIDs, but ownership is not enforced on the unauthenticated cart path. Requires two test accounts to test object-level authorization safely.
3. **Order-total manipulation via cart price.** FT-02 confirms price trust at the cart layer. Whether `POST /api/orders` or checkout uses the manipulated cart price was not tested. Requires an authenticated test account and a controlled test-mode checkout; no real payment.
4. **Client-supplied discount.** The `CreateOrderDto` schema includes a `discount` field. The frontend does not appear to send it, but a client could. Whether the server trusts it requires authenticated testing.
5. **Open redirect via `returnUrl`.** The login component appears to support a `returnUrl` parameter. Server-side validation was not tested because successful login requires an account.
6. **Stored/authenticated XSS.** No reflected XSS was confirmed in unauthenticated API error handling. Product, cart, address, and order fields may be rendered in authenticated views; these contexts were not tested.
7. **Authentication hardening.** Rate limiting, account enumeration, password policy, lockout, MFA enforcement, refresh-token rotation, and password-reset abuse were not tested.
8. **Admin BFLA.** Unauthenticated admin endpoints returned `401`, which is expected. Role separation and function-level authorization require an admin test account and a normal user account.

## 6. Attack Surface and Route Inventory

### Frontend routes (from delivered `main.js`)
- `/`
- `/shop`
- `/shop/:id`
- `/Cart`
- `/custmize`
- `/products/add`
- `/products/edit/:id`
- `/products/delete/:id`
- `/checkout`
- `/checkout/success`
- `/orders`
- `/orders/:id`
- `/account/login`
- `/account/register`
- `/test-error`
- `/not-found`
- `/server-error`
- `/admin`
- `**` → not-found

All tested SPA routes returned the same `200 text/html` shell from the Vite server, which is expected for a client-side application.

### API endpoints (from Swagger)
- `/api/Account/Register` `[POST]`
- `/api/Account/logout` `[POST]`
- `/api/Account/user-info` `[GET]`
- `/api/Account/auth-status` `[GET]`
- `/api/Account/address` `[POST]`
- `/api/Admin/orders` `[GET]`
- `/api/Admin/orders/{id}` `[GET]`
- `/api/Admin/orders/refund/{id}` `[POST]`
- `/api/Buggy/unauthorized` `[GET]`
- `/api/Buggy/badrequest` `[GET]`
- `/api/Buggy/Notfound` `[GET]`
- `/api/Buggy/internalerror` `[GET]`
- `/api/Buggy/validationerror` `[POST]`
- `/api/Buggy/secret` `[GET]`
- `/api/Buggy/admin-secret` `[GET]`
- `/api/Cart` `[GET, POST, DELETE]`
- `/api/register` `[POST]`
- `/api/login` `[POST]`
- `/api/refresh` `[POST]`
- `/api/confirmEmail` `[GET]`
- `/api/resendConfirmationEmail` `[POST]`
- `/api/forgotPassword` `[POST]`
- `/api/resetPassword` `[POST]`
- `/api/manage/2fa` `[POST]`
- `/api/manage/info` `[GET, POST]`
- `/api/orders` `[POST, GET]`
- `/api/orders/{id}` `[GET]`
- `/api/Payment/{cartId}` `[POST]`
- `/api/Payment/delivery-methods` `[GET]`
- `/api/Payment/webhook` `[POST]`
- `/api/product` `[POST]`
- `/api/product/{id}` `[PUT, DELETE]`
- `/api/ProductQueries` `[GET]`
- `/api/ProductQueries/{id}` `[GET]`
- `/api/ProductQueries/type` `[GET]`
- `/api/ProductQueries/brand` `[GET]`

### Authentication and session observations
- Login uses `?useCookies=true`.
- The frontend uses `withCredentials: true` for API calls.
- A nonexistent account returned `401` with a generic `"Failed"` detail and no `Set-Cookie`.
- A malformed login body caused a `500` stack trace.
- No successful login, cookie attributes, session fixation, logout invalidation, or refresh behavior was tested.
- The frontend stores `cart_id` client-side; no bearer token was observed in local storage.

## 7. Tested Without Confirmed Vulnerability

### SQL injection
- `/api/ProductQueries` search parameter: `'`, `paint'`, `paint'--`, boolean true/false, and `' OR '1'='1` returned normal or empty result sets with no SQL errors.
- `/api/ProductQueries` `sort` parameter: quote and expression payloads returned the same data as baseline; no error or injection effect.
- `brands` and `types` filters: quote and boolean payloads returned empty results; no SQL errors.
- `POST /api/login` with email `' OR '1'='1`: `401` generic failure; no SQL error.
- **Conclusion:** No SQL injection was confirmed in the tested unauthenticated parameters. This is not proof of safety for all endpoints.

### Cart ID input
- `/api/Cart?id='`, `?id=1' OR '1'='1`, and `?id=not-a-guid` returned `200` with the supplied value echoed and empty items.
- This indicates missing identifier validation and client-controlled cart IDs, not SQL injection.

### Authentication
- Invalid login returned `401` with a generic response.
- Protected endpoints returned `401` without authentication:
  - `/api/orders`
  - `/api/orders/1`
  - `/api/admin/orders`
  - `/api/admin/orders/1`
  - `/api/Buggy/secret`
  - `/api/Buggy/admin-secret`
  - `/api/manage/info`

### Cross-site scripting (XSS)
- No browser-based XSS payload execution was performed.
- Reflected XSS was not observed in the API error responses reviewed, but output encoding in the Angular UI was not verified with a browser.
- Authenticated and stored XSS in product, cart, address, order, and profile fields remains untested.
- The delivered JavaScript bundle was reviewed only for high-level route/API/path exposure, not for exhaustive DOM-sink analysis.

### SPA routes
- `/shop`, `/account/login`, `/account/register`, `/checkout`, `/orders`, `/admin`, `/not-found`, `/test-error`, and `/server-error` all returned the same frontend shell. This is expected for an SPA and does not indicate server-side authorization issues.

## 8. Limitations

- **No dedicated test accounts were supplied.** Successful login, cookie attributes, session lifecycle, CSRF, IDOR/BOLA, BFLA, stored XSS, and authenticated business logic remain untested.
- **No checkout or payment was performed.** Order-total impact from cart price manipulation is not confirmed.
- **No source maps or `/@fs/...` content was fetched or decoded.** Vite source disclosure is assessed as potential, not confirmed.
- **No browser-based testing.** DOM XSS, client-side route guards, and browser-enforced CORS behavior were not tested with a real browser.
- **No preflight (`OPTIONS`) or `TRACE` testing.** CORS preflight behavior and allowed methods/headers were not enumerated.
- **No denial-of-service, load, race-condition, or brute-force testing.**
- **TLS verification bypass was used.** Certificates are development/self-signed and not valid for `192.168.1.20`; this is an environment limitation, not an application vulnerability.
- **The environment appears to be development/training.** Some findings may be intentional for that context; production deployment would change severity and exploitability.

## 9. Remediation Priorities

1. **Fix cart price handling immediately.** Resolve prices server-side, reject negative prices, compute totals server-side, and revalidate at checkout.
2. **Fix CORS.** Use an exact allowlist; never reflect arbitrary origins or `null` with credentials enabled.
3. **Disable verbose error responses.** Return generic errors with correlation IDs; validate pagination before database access.
4. **Remove public Swagger and the Vite dev server from network exposure.** Serve a production build; disable source maps.
5. **Add security headers.** HSTS, CSP, frame protection, `nosniff`, referrer policy, and permissions policy.
6. **Harden cart and order authorization.** Bind carts/orders to sessions or users; enforce object-level authorization.
7. **Complete authenticated testing.** Provide test accounts and approve the authenticated test plan.

## 10. Requested Approvals / Next Phase

To complete the assessment, the following are requested:

1. **Two dedicated test accounts** (user A and user B) and, if available, one admin test account. Credentials should be provided through an approved secure channel, not in this report.
2. **Approval for authenticated tests:**
   - Login success and cookie attributes (`HttpOnly`, `Secure`, `SameSite`).
   - Logout and session invalidation.
   - CSRF behavior on state-changing endpoints.
   - IDOR/BOLA on `/api/orders/{id}`, `/api/Cart`, and `/api/Account/address`.
   - BFLA between normal user and admin roles.
   - Stored/authenticated XSS in product, cart, address, and order fields.
3. **Approval for a controlled order-price test** using a manipulated test cart and a test-mode checkout only. No real payment or financial transaction will be performed.
4. **Decision on Vite source disclosure testing.** Retrieval of `/@fs/...` or `.map` content is currently prohibited. If approved, this can confirm or rule out source-code disclosure.
5. **Confirmation of test data.** Confirm that product ID `1965` and the test cart workflow may be used for authenticated order tests.

## 11. Evidence Index

The canonical evidence index and hashes are stored at:

- `11-report\evidence-index.txt`
- `11-report\evidence-index.sha256`

Key evidence files by finding:

| Finding | Evidence |
|---|---|
| FT-01 CORS | `05-surface\cors\cors-log.txt` and per-origin header files |
| FT-02 Cart price | `09-business\cart-price-test\cart-price-test-log.txt` and request/response files |
| FT-03 Verbose errors | `05-surface\unauth-get\api-16.body.bin`, `06-auth\login-invalid.body.bin`, `08-input\validation-productqueries\validation-log.txt` |
| FT-04 Swagger | `05-surface\docs\docs-request-log.txt`, `swagger-summary.txt`, `swagger-parameters.txt`, `swagger-schemas.txt` |
| FT-05 Vite exposure | `05-surface\asset-fetch.txt`, `05-surface\assets\main.js`, `styles.css`, `vite-client.js` |
| FT-06 Missing headers | `04-http\baseline-4200.headers.txt`, `04-http\baseline-5001.headers.txt` |
| FT-07 Cart authorization | `05-surface\unauth-get\request-log.txt`, `08-input\sqli-cart-id\cart-id-log.txt`, `09-business\cart-price-test\cart-price-test-log.txt` |
| SQLi negatives | `08-input\sqli-productqueries\sqli-log.txt`, `08-input\sqli-filters\filter-sqli-log.txt`, `08-input\sqli-login\login-sqli-log.txt` |
| TLS inventory | `03-tls\cert-inventory.txt`, `03-tls\tls-validation-4200.txt`, `03-tls\tls-validation-5001.txt` |
| Scope | `00-scope\scope-ledger-active.txt` |

all files are kept locally as it contain sensitive data .
