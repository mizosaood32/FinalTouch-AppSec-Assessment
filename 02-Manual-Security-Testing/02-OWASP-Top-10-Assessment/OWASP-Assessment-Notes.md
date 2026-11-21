## A01 for BAC 

I tested each account acc1 and acc2 
each have its own cookie nothing leaked no role shown ,

cant access the cart of the other 

neither the orders made by acc1 on acc2

check if cookie expire can be used again
it didnt work on test1 acc and test2 acc

as we knew the api we can access it and see if it needs auth or not
```
GET /api/ProductQueries?sort=name&pageSize=100&pageIndex=1 HTTP/2
Host: 192.168.1.20:5001
```
lets try admin or orders 
```
GET /api/admin/orders?pageIndex=1&pageSize=10 HTTP/2
Host: 192.168.1.20:5001
```


The administrative orders endpoint returned authorized results only when accessed using the administrator's session. Requests using regular user sessions were unsuccessful.

but i used the url 
/api/Account/user-info
to get info about the users i found that it only show the authenticated user info only 
![user-info](Evidence/Authentication/user-info.png)

so i thought of changing roles here 
but didnt work like using post or patch or put on the same api 
i know it wont work as we set it to be get and post as the respond was 405 method not allowed
even tried it on the post request of the sign up 
it didnt accept the roles to be admin 
but when set it to roles =1 it accepted it although the request body itself didnt have roles in it 
i added it manually 

![role-added-manually](Evidence/Business-Logic/role-added-manually.png)
then i noticed i didnt use quotes mark
then 
![role-admin](Evidence/Business-Logic/role-admin.png)
but like it didnt get the admin role even the roles didnt register or save anywhere as we already made the admin manually i remember on the site and gave him the role of admin manually not by a register post request like here

i think thats enough for BAC
so the result we have is 
- there is method restriction
- extra field in register is accepted but ignored
- no privilege escalation so far 

other findings

but for the IDOR we used
the stripe payment flow had a connection to the price with the id of the item 
and if the id changed in the transaction it can use another price of another item but the same item is bought normally 
thats a business logic flaw expressed before

---
## A02 security misconfig 

as we are testing in development phase 
so the swagger he can be viewed normally 
5001/swagger its not a vuln only an observation

also the cors 
that i edited to work on any 
```
builder.Services.AddCors(options =>

{

    options.AddPolicy("AllowAngularApp", policy =>

    {

        policy.SetIsOriginAllowed(_ => true)

              .AllowAnyHeader()

              .AllowAnyMethod()

              .AllowCredentials();
```

here the `_=>true` will work as wildcard like allow all origins so any site will be allowed to receive  data to the server 
This permissive CORS policy was introduced temporarily during lab setup to enable VM connectivity. It was not treated as a confirmed vulnerability in the application's original configuration.
its better to use in production like allowed policies or host.endswith()

---
## A03 software supply chain failure

in the front end (Angular 18)
we used 
`npm audit` found
85 vulnerabilities (5 low, 25 moderate, 50 high, 5 critical)
The frontend dependency tree contained multiple packages with reported security advisories. The dependency audit identified 85 vulnerabilities across different severity levels. Individual affected packages and advisories require review to establish their actual impact.

while on the server (.NET 8 Web API)
`dotnet list package --vulnerable`
had no vuln package which is great 


---
## A04  cryptographic failures

passwords were hashed as shown up 

![Password-Hash](Evidence/Business-Logic/Password-Hash.png)
Identical passwords produced different stored password hashes, which is consistent with salted password hashing. The exact hashing algorithm and its configuration were not independently verified.

also nothing big like only shows that it uses .net identity for cookie header

![Identity](Evidence/Business-Logic/Identity.png)

---
## A05  Injection

Initial XSS payloads were tested through URL parameters and search functionality. No confirmed reflected or stored XSS was identified. A DOM-related behavior produced an unexpected page response, but JavaScript execution was not confirmed. SQL injection testing was limited and remains incomplete.

Additional input manipulation attempts were performed against cart-related requests. No confirmed injection vulnerability was identified during these tests.

---
## A06   Insecure Design

**Confirmed observation: Checkout business-logic weakness**
During checkout testing, modifying the product identifier allowed an order to retain the original expensive product information while using the cheaper product's price.

The behavior was verified through:

- The application checkout result.
    
- The resulting order record in the database.
    
- The Stripe test payment record.
    

This indicates insufficient integrity validation between product identity and price during the checkout process.
![successful-order-result](Evidence/Business-Logic/successful-order-result.png)
![database-order-verification](Evidence/Business-Logic/database-order-verification.png)

**Additional test: Concurrent sessions**
Two sessions using the same account were opened on separate devices and progressed to the payment stage.

The application accepted the sessions, but the observed payment workflow did not demonstrate two independently successful payments. The last active tab appeared to control the accepted payment flow.

A separate test sent five simultaneous checkout-related requests. The requests returned `400 Bad Request` rather than producing multiple successful orders i checked in database .
![Race-condition](Evidence/Business-Logic/race-condition.png)
![error400](Evidence/Business-Logic/error400.png)

**Conclusion:** The checkout product/price integrity weakness was confirmed. No successful duplicate-order or duplicate-payment condition was demonstrated during the concurrency tests.

---
## A07 authentication failure

Multiple invalid login attempts were performed using Burp Suite Intruder. No apparent rate limiting, temporary lockout, or progressive delay was observed during the test. This may increase exposure to automated password-guessing attempts and requires further validation.
Authentication is well formed , registration is good but needs some more features like 0auth and two factor auth as a possible improvement .
also logout users cant access api endpoints 

---
## A08  software and data integrity failures

same vuln from A06
as we said earlier here the item id is connected to the price in the payment order if manipulated it can make us order the item with different price 
i saw in the code its written 
its correct for frontend as it still checks backend . 
Although the backend performs a price comparison before creating the PaymentIntent, the resulting order and payment records contained inconsistent product and price information. Further code tracing is required to identify the exact validation or data-mapping failure.
```
foreach (var item in cart.Items)

            {

                var productItem = await unit.QueryRepository<Core.Entities.Product>().GetByIdAsync(item.ProductId);

                if (productItem == null) return null;

                if (item.Price != productItem.Price)

                {

                    item.Price = productItem.Price;

                }

            }

            var service = new PaymentIntentService();

            PaymentIntent? intent = null;

  

            if (string.IsNullOrEmpty(cart.PaymentIntentId))

            {

                var options = new PaymentIntentCreateOptions

                {

                    Amount = (long)cart.Items.Sum(x => x.Quantity * (x.Price * 100)) + (long)shippingPrice * 100,

                    Currency = "usd",

                    PaymentMethodTypes = ["card"]

                };}
```

rather than that the database store the no. of the order 
and on stripe also 


---
## A09  security logging and alert failure

During the assessment, the ASP.NET Core server console displayed operational logs, including HTTP request processing, endpoint execution, middleware activity, CORS processing, and database-related output.

These logs provided visibility into normal application operations and request handling. However, the assessment did not establish whether security-relevant events, such as repeated failed authentication attempts, authorization failures, suspicious account activity, and checkout manipulation, were consistently recorded or monitored.

A complete review of centralized logging, security event monitoring, alerting rules, and log retention was not performed.

Conclusion: Operational logging was observed, but the effectiveness and coverage of security logging and alerting remain unverified. Further assessment is required before classifying this as a confirmed security logging weakness.

---
## A10 mishandling of exceptional condition 

A malformed URL containing an invalid encoded character triggered a development error response that exposed local file-system paths and development-related information. This was observed in the development environment and requires verification in a production configuration.

Additional tests involving invalid identifiers, forged cookies, and missing request fields produced 400 or 404 responses. No additional vulnerability was confirmed from these responses.

---

## Overall Assessment Conclusion

The FinalTouch application underwent a manual security assessment covering the OWASP Top 10 (2025) categories, including authentication, authorization, input handling, business logic, application configuration, and dependency security.

The assessment confirmed a checkout product/price integrity weakness that allowed an order to retain the original product information while using a different product's price.

Additional observations included frontend dependency vulnerabilities, development-related error disclosure, and potential authentication rate-limiting and logging limitations.

Several tested areas, including cross-account access, role manipulation, and concurrent checkout requests, did not produce confirmed vulnerabilities during the assessment.

The results will be used to prioritize remediation, review frontend dependencies, and perform security retesting against a separate secured copy of the application.



