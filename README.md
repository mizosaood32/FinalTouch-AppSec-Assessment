# 🔐 FinalTouch – Application Security Assessment

**A Practical Black-Box & White-Box Security Assessment of a .NET and Angular Web Application**

This repository documents a practical security assessment of **FinalTouch**, a web application built using ASP.NET Core and Angular.

The project examines application security from two complementary perspectives:

- **Black-Box Assessment:** Evaluating the application externally, without relying on its internal source code.
- **White-Box Assessment:** Examining source code, dependencies, and implementation patterns to identify potential weaknesses.

The assessment combines reconnaissance, manual security testing, automated Dynamic Application Security Testing (DAST), AI-assisted Black-Box testing, Static Application Security Testing (SAST), Software Composition Analysis (SCA), manual code review, and AI-assisted source-code review.

The objective is not simply to collect scanner alerts. It is to understand the application, identify potential weaknesses, validate their actual security impact, and document evidence-based findings and remediation recommendations.

---

## 📌 Project Overview

| Attribute           | Details                      |
| ------------------- | ---------------------------- |
| Application         | FinalTouch                   |
| Assessment Type     | Black-Box & White-Box        |
| Backend             | ASP.NET Core / .NET 8        |
| Frontend            | Angular / TypeScript         |
| Database            | SQL Server                   |
| Primary Focus       | Web Application Security     |
| Testing Environment | Authorized Local Virtual Lab |

### Assessment Architecture

The assessment is organized into two main approaches, each containing three assessment activities.

```text
                         FinalTouch Application
                                  |
                 +----------------+----------------+
                 |                                 |
             BLACK-BOX                         WHITE-BOX
          External Perspective              Internal Perspective
                 |                                 |
     +-----------+-----------+         +-----------+-----------+
     |           |           |         |           |           |
     v           v           v         v           v           v
  1. Manual   2. Automated  3. AI     1. SAST    2. SCA    3. AI-Assisted
     Testing     Testing     Security                       Code Review
                            Testing
     |           |           |         |           |           |
     v           v           v         v           v           v
  02-Manual-  03-Automated  04-AI-    05-White-  05-White-  05-White-
  Security-   -Security-   Assisted-    Box/       Box/       Box/
  Testing      Testing    Black-Box   02-SAST    04-SCA    05-AI-
                                                            Assisted-
                                                            Code-Review
     |           |           |         |           |           |
     +-----------+-----------+---------+-----------+-----------+
                                  |
                                  v
                       06-Findings Evaluation
                                  |
                                  v
                       Evidence & Documentation
                                  |
                                  v
                       07-Remediation & Retesting
```

The two approaches provide different perspectives on the same application.

Black-Box testing evaluates what an external user or attacker can discover through the application's exposed interfaces.

White-Box testing examines the implementation itself, identifying potentially insecure code patterns, vulnerable dependencies, and weaknesses that may not be visible through external testing alone.

---

# 🔴 01. Black-Box Security Assessment

**Objective:** Evaluate the application from an external attacker's perspective without relying on direct access to its internal source code during the assessment.

The Black-Box assessment was performed against the locally hosted application within an authorized testing environment.

It consists of three complementary activities:

1. Manual Security Testing
2. Automated Security Testing
3. AI-Assisted Black-Box Security Testing

## 1.1 Manual Security Testing

### Phase 1 – Network Reconnaissance & Discovery

The initial stage focused on identifying exposed services, understanding the application's attack surface, and discovering the technologies behind it.

**Activities:**

- Network and port scanning.
- Service and version identification.
- Web directory and endpoint discovery.
- Technology fingerprinting.
- Initial attack-surface mapping.

**Tools:**

- Nmap
- Gobuster / FFUF
- Wappalyzer

### Phase 2 – Application Exploration

After identifying the application and its technologies, the next stage focused on understanding its functionality and application workflows.

**Activities:**

- Exploring registration and authentication.
- Understanding user and administrator functionality.
- Mapping application routes and endpoints.
- Inspecting HTTP requests and responses.
- Identifying security-sensitive operations.
- Exploring shopping cart, checkout, and order workflows.

**Tools:**

- Burp Suite
- Browser Developer Tools

### Phase 3 – OWASP Top 10 & Manual Security Testing

The application was examined for common web application security weaknesses, following the OWASP Top 10 as a testing reference.

**Areas examined:**

- Authentication and session management.
- Broken Access Control (BAC).
- Cross-user data isolation.
- Input validation and injection testing.
- Cross-Site Scripting (XSS).
- SQL Injection (SQLi).
- Cross-Origin Resource Sharing (CORS).
- Business Logic vulnerabilities.
- Application security headers and configuration.

**Tools:**

- Burp Suite
- Browser Developer Tools
- Manual HTTP request manipulation

### Notable Manual Testing Observations

**Business Logic – Product and Checkout Inconsistency**

During checkout testing, a product substitution scenario was identified where the order retained the information and image of a more expensive product while the associated product ID corresponded to a cheaper item.

The inconsistency was verified through order records and database inspection.

This highlighted the importance of validating product identity, price, and order relationships on the server side rather than trusting client-side application data.

**Access Control Testing**

Two separate user accounts were used to evaluate cross-user access.

Testing included:

- Attempting to access another user's cart.
- Attempting to retrieve another user's orders.
- Checking account-specific session behavior.
- Evaluating administrative functionality.

The tested user accounts maintained separation of their respective carts and orders. Administrative functionality was evaluated separately.

**Important:** Testing a security control without successfully bypassing it is still useful assessment evidence. It should be documented as a tested behavior, not reported as a confirmed vulnerability.

---

## 1.2 Automated Security Testing

**Objective:** Use automated security tools to identify potential vulnerabilities and security misconfigurations that require further investigation and validation.

Automated testing complements manual testing by providing broader coverage and identifying issues that may otherwise be overlooked.

### OWASP ZAP – Dynamic Application Security Testing

OWASP ZAP was used to perform dynamic analysis against the running application.

**Activities:**

- Application crawling and spidering.
- Discovering accessible routes and resources.
- Passive security analysis.
- Reviewing HTTP responses and security headers.
- Identifying potential configuration weaknesses.
- Reviewing and triaging scanner alerts.

**Areas highlighted by the scan included:**

- Application error disclosure.
- Content Security Policy (CSP) configuration.
- CORS configuration.
- Missing clickjacking protection headers.
- Missing Subresource Integrity (SRI).
- Cookie SameSite attributes.
- Server information disclosure.
- HSTS configuration.

The initial ZAP assessment reported:

| Alert Risk    | Number of Alerts |
| ------------- | ---------------: |
| Medium        |                6 |
| Low           |                8 |
| Informational |               11 |

These are scanner alert counts, not counts of independently confirmed vulnerabilities. Each relevant alert requires manual review to establish its validity and impact.

### Nuclei – Automated Vulnerability Scanning

Nuclei is included in the automated testing phase for template-based security checks against the authorized application.

The Nuclei results and supporting evidence are maintained separately under the automated testing directory.

**Assessment principle:** Automated results are treated as leads for investigation, not automatic proof of exploitability.

---

## 1.3 AI-Assisted Black-Box Security Testing

**Objective:** Evaluate how an AI-assisted security agent can independently examine an application's externally exposed attack surface, discover potential vulnerabilities, and produce evidence-based security observations.

This activity is distinct from the White-Box AI-assisted code review.

The Black-Box AI assessment was designed to operate from the perspective of an external tester, using explicitly supplied application URLs rather than examining the application's source code.

### Tooling & Execution Environment

- **AI execution environment:** OpenCode
- **Main model selected:** DeepSeek V4.1 Flash Free
- **Alternative considered:** GLM 5.3 Free
- **Execution command:** `/audit`
- **Assessment target:** Explicitly supplied authorized FinalTouch URLs.

The AI assessment was run from a separate assessment workspace rather than being given unrestricted access to the application repository.

### Assessment Workflow

The workflow was organized into controlled phases:

**Phase 1 – Planning**

The AI agent was instructed to:

- Review the authorized target scope.
- Understand the Black-Box testing restrictions.
- Identify the available testing surface.
- Develop a structured assessment plan.
- Prioritize passive and low-impact testing.
- Identify actions that would require additional approval.

The planning phase was intended to produce an execution plan before performing the assessment.

**Phase 2 – Build / Execution**

After reviewing the plan, the execution phase was used to carry out the permitted assessment activities.

The agent was instructed to:

- Follow the approved scope and testing plan.
- Examine externally accessible application behavior.
- Collect relevant evidence.
- Maintain a record of discovered routes and observations.
- Document potential vulnerabilities and their supporting evidence.
- Stop at approval gates instead of proceeding with restricted actions.

**Phase 3 – Findings Review & Triage**

AI-generated observations were not automatically accepted as vulnerabilities.

The generated report was reviewed to distinguish:

- Confirmed findings.
- Potential findings requiring additional validation.
- False positives or unsupported observations.
- Informational observations.
- Testing limitations and areas requiring further investigation.

### Security Restrictions & Guardrails

The AI assessment was deliberately constrained to maintain a genuine Black-Box approach and prevent uncontrolled testing.

**Scope restrictions:**

- Only explicitly supplied authorized target URLs were in scope.
- No source-code or project-file inspection.
- No access to internal application configuration.
- No silent expansion of the testing scope.
- No assumption that an observed behavior represents a vulnerability.

**Execution restrictions:**

- Passive and low-impact testing first.
- No destructive testing without explicit approval.
- No unapproved state-changing operations.
- No unapproved payment or checkout actions.
- No unauthorized modification of production data.
- Additional approval required before proceeding with restricted active testing.

**Reporting requirements:**

Each potential finding should include, where available:

- Description of the observed behavior.
- Affected endpoint or application component.
- Supporting evidence.
- Reproduction steps.
- Potential security impact.
- Confidence level.
- Limitations and validation requirements.

### Documentation & Evidence

The AI-assisted Black-Box assessment is documented separately to preserve its methodology, execution instructions, generated report, and findings triage.

The following materials are maintained under:

`04-AI-Assisted-Black-Box/`

- Assessment methodology and execution instructions.
- Audit command and planning/build prompts.
- Run metadata.
- Assessment evidence.
- Generated assessment report.
- Findings triage.

The purpose of this activity is to evaluate AI as an additional security testing assistant, not to replace independent technical validation.

---

# 🔵 02. White-Box Security Assessment

**Objective:** Examine the application's internal implementation to identify weaknesses that may not be discoverable through external testing alone.

The White-Box assessment examines source code and dependencies using three complementary activities:

1. Static Application Security Testing (SAST)
2. Software Composition Analysis (SCA)
3. AI-Assisted Security Code Review

## 2.1 Static Application Security Testing (SAST)

Source-code analysis was performed using **SonarQube** to identify potential security issues, reliability problems, and insecure implementation patterns.

### Scope

- ASP.NET Core backend.
- API and middleware.
- Application and business logic layers.
- Database interaction patterns.
- Security-sensitive implementation areas.

### Activities

- Automated static code analysis.
- Reviewing scanner-generated issues.
- Classifying security-relevant observations.
- Distinguishing security weaknesses from code-quality recommendations.
- Documenting potential remediation.

### Notable Observations

**Production Exception Handling**

The exception-handling middleware returns the original exception message even in the production response.

This may disclose internal implementation details or sensitive diagnostic information.

The behavior requires remediation by returning generic client-facing error messages while retaining detailed exception information in server-side logs.

**Missing Cancellation Token Propagation – S8949**

A cancellation token was not propagated to an asynchronous response-writing operation.

This is primarily a reliability and code-quality observation rather than an independently established security vulnerability.

**Synchronous Database Operation**

A synchronous `SaveChanges()` call was identified where asynchronous database operations may be more appropriate.

This is a performance and reliability recommendation.

**Application Execution Pattern**

A synchronous `app.Run()` call was identified by the analysis tool as an opportunity to use the asynchronous execution pattern.

This is a code-quality recommendation, not a confirmed vulnerability.

**Stripe Webhook Request Handling**

A scanner recommendation concerned raw request handling in the Stripe webhook implementation.

This requires contextual review because Stripe signature verification depends on the original, unmodified request body and the signature header.

The recommendation should not be applied blindly if it interferes with signature validation.

Scanner recommendations were evaluated against the actual implementation rather than accepted without review.

---

## 2.2 Software Composition Analysis (SCA)

**Objective:** Identify known vulnerabilities in third-party packages and dependencies used by the application.

The dependency assessment covers both the backend and frontend package ecosystems.

### Scope

- NuGet dependencies.
- npm dependencies.
- Backend and frontend package ecosystems.

### Tools & Commands

**.NET / NuGet**

```powershell
dotnet list package --vulnerable
```

The .NET vulnerable-package check was executed, with no vulnerable NuGet packages reported in the recorded assessment.

**Angular / npm**

```bash
npm audit
```

The frontend dependency audit is maintained separately. Its final results should be documented according to the actual npm audit output.

### Assessment Considerations

- Package name and installed version.
- Direct or transitive dependency.
- Known vulnerability advisory or CVE.
- Severity and potential impact.
- Available patched version.
- Recommended remediation.
- Whether the vulnerable dependency is actually used by the application.

The presence of an outdated package does not automatically establish an exploitable vulnerability. Findings should be evaluated against the relevant advisory and application context.

---

## 2.3 AI-Assisted Security Code Review

**Objective:** Evaluate the ability of AI models to support source-code security reviews by identifying potentially vulnerable implementation patterns and weaknesses in application logic.

Unlike the AI-assisted Black-Box assessment, this activity explicitly examines application source code.

### Assessment Focus

- Authentication and authorization implementation.
- Access control and ownership validation.
- Input validation and data handling.
- Injection-related implementation patterns.
- Business logic and checkout integrity.
- Payment-related workflows.
- Database interactions.
- Error handling and security-sensitive operations.

### Planned Review Workflow

**Phase 1 – Planning**

Identify relevant source-code components and security-sensitive areas for review.

**Phase 2 – Code Analysis**

Review selected implementation areas and generate potential security observations.

**Phase 3 – Validation**

Independently evaluate generated observations against the actual source code and application behavior.

### Validation Principles

- AI-generated output is treated as an assessment aid.
- Findings must be supported by relevant code or behavioral evidence.
- Context-dependent recommendations must be reviewed before acceptance.
- Duplicate findings and unsupported claims should be removed.
- Security impact must be established before classifying an issue as a vulnerability.

The AI-assisted code review is kept separate from the AI Black-Box assessment because the two activities have different access levels, objectives, and testing methodologies.

---

# 🧪 03. Findings Evaluation & Validation

Identifying a potential vulnerability is only the beginning of a security assessment.

A scanner may identify a suspicious pattern, and an AI model may suggest a weakness, but neither automatically establishes exploitability or security impact.

Throughout this project, findings are evaluated according to:

- Actual implementation and application context.
- Reproducibility of the observed behavior.
- Available technical evidence.
- Potential security impact.
- Whether the issue represents a genuine vulnerability, a potential weakness, or a code-quality recommendation.
- Whether the observation is duplicated across multiple assessment methods.

### Finding Classification

| Classification              | Meaning                                                                        |
| --------------------------- | ------------------------------------------------------------------------------ |
| Confirmed Vulnerability     | A security weakness supported by sufficient validation evidence.               |
| Potential Security Weakness | A suspicious implementation or behavior requiring further validation.          |
| Code Quality / Reliability  | An improvement that does not independently establish a security vulnerability. |
| Informational               | An observation that provides useful technical context.                         |

### Consolidated Findings

Findings from the different assessment activities are organized centrally under:

`06-Findings/`

This separates individual testing results from the consolidated findings and recommendations.

The directory contains:

- Confirmed findings.
- Potential findings.
- Security recommendations.

Each finding should contain enough technical context and supporting evidence to allow another reviewer to understand and reproduce the issue.

---

# 🛠️ Tools & Technologies

| Category                         | Tools / Technologies              |
| -------------------------------- | --------------------------------- |
| Network Reconnaissance           | Nmap                              |
| Web Discovery                    | Gobuster, FFUF                    |
| Technology Fingerprinting        | Wappalyzer                        |
| Manual Web Testing               | Burp Suite                        |
| Dynamic Analysis (DAST)          | OWASP ZAP                         |
| Automated Vulnerability Scanning | Nuclei                            |
| Static Analysis (SAST)           | SonarQube                         |
| Dependency Analysis (SCA)        | NuGet, npm                        |
| AI-Assisted Black-Box Testing    | OpenCode, DeepSeek V4.1 Flash     |
| AI-Assisted Code Review          | OpenCode, DeepSeek V4.1 Flash     |
| Application Stack                | ASP.NET Core, Angular, SQL Server |
| Testing Environment              | Kali Linux, Virtual Machines      |

---

# 📂 Repository Structure

The repository is organized according to the assessment workflow, keeping the application, testing methodologies, evidence, AI-assisted assessments, consolidated findings, and remediation documentation separate.

```text
FinalTouch-AppSec-Assessment/
│
├── 01-Vulnerable-Application/
│   ├── FinalTouch.ClientSide/
│   ├── FinalTouch.ServerSide/
│   ├── package.json
│   └── package-lock.json
│
├── 02-Manual-Security-Testing/
│   ├── 01-Initial-Exploration/
│   │   └── Evidence/
│   │       ├── Authentication/
│   │       ├── Business-Logic/
│   │       ├── Reconnaissance/
│   │       ├── Technology-Fingerprinting/
│   │       └── Web-Discovery/
│   │
│   └── 02-OWASP-Top-10-Assessment/
│
├── 03-Automated-Security-Testing/
│   ├── Nuclei/
│   └── ZAP/
│       └── Evidence/
│
├── 04-AI-Assisted-Black-Box/
│   ├── 01-Methodology/
│   ├── 02-Assessment-Report/
│   └── 03-Findings-Triage/
│
├── 05-White-Box/
│   ├── 01-Methodology/
│   ├── 02-SAST/
│   ├── 04-SCA/
│   ├── 05-AI-Assisted-Code-Review/
│   └── 06-Findings-Triage/
│
├── 06-Findings/
│   ├── Confirmed-Findings/
│   ├── Potential-Findings/
│   └── Recommendations/
│
└── 07-Remediation-and-Retesting/
```

*The structure above follows the repository organization. Individual reports, evidence files, and methodology documents are maintained inside their respective directories.*

---

# 🎯 Why Both Approaches Matter

Black-Box and White-Box testing are not competing methodologies. They complement one another.

| Black-Box                                                       | White-Box                                                          |
| --------------------------------------------------------------- | ------------------------------------------------------------------ |
| Examines externally exposed behavior.                           | Examines internal implementation.                                  |
| Identifies exploitable application behavior.                    | Identifies potentially insecure code and dependencies.             |
| Tests authentication, authorization, and business logic.        | Reviews security-sensitive code paths and implementation patterns. |
| Validates real application responses.                           | Helps understand the underlying causes of weaknesses.              |
| Uses manual testing, DAST, and AI-assisted external assessment. | Uses SAST, SCA, manual code review, and AI-assisted code review.   |

A vulnerability might be visible through an application's behavior without its underlying cause being immediately obvious.

Likewise, a suspicious implementation may be identified through source-code analysis without necessarily being exploitable in practice.

Combining both approaches provides a more complete understanding of application security.

**The key lesson:** Effective application security is not simply about finding scanner alerts. It is about understanding the application, identifying weaknesses, validating their impact, and connecting technical findings to the underlying implementation.

---

# 🔧 04. Remediation & Retesting

The final stage focuses on translating validated findings into practical security improvements.

Remediation recommendations should address the underlying cause of each finding rather than simply suppressing scanner alerts.

Retesting should verify that:

- The original issue has been addressed.
- The affected functionality continues to operate correctly.
- The vulnerability can no longer be reproduced under the original test conditions.
- The remediation has not introduced a new security or functional issue.

The remediation and retesting documentation is maintained under:

`07-Remediation-and-Retesting/`

---

# ⚠️ Scope & Disclaimer

This project was conducted in an authorized local testing environment against the FinalTouch application.

All testing activities were performed for educational and security assessment purposes.

- No unauthorized third-party systems were targeted.
- Black-Box AI testing was restricted to explicitly supplied authorized targets.
- Source-code access was reserved for the White-Box assessment activities.
- Destructive, state-changing, and payment-related testing required explicit approval.
- Findings and scanner observations are documented according to the available evidence.
- Automated and AI-generated results require independent validation.
- Assessment results reflect the application state at the time of testing.

---

# 📚 Project Status

This repository documents the practical assessment process, methodologies, findings, and supporting evidence.

The Black-Box assessment covers reconnaissance, application exploration, OWASP Top 10 testing, automated security analysis, and AI-assisted external testing.

The White-Box assessment covers static analysis, dependency analysis, manual code review, and AI-assisted source-code review.

The findings are consolidated separately from the individual assessment reports to preserve traceability between observations, evidence, validation, and remediation.

Some analysis and validation activities remain in progress. Their results will be documented as they are completed.

---

**FinalTouch – Application Security Assessment**

*Understanding the application from the outside in — and the inside out.* 🔐
