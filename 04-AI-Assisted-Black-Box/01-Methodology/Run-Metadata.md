# AI Assessment — Run Metadata

## Assessment Information

- Project: FinalTouch
- Assessment type: AI-assisted black-box security testing
- Execution date: 11 November 2026
- Environment: Local development environment
- AI environment: OpenCode
- Command: `/audit`
- AI Model : DeepSeek V4.1 Flash

## Target Scope

- Frontend: `https://192.168.1.20:4200`
- Backend API: `https://192.168.1.20:5001`

## Execution Method

- Planning phase: Two prompts.
- Build phase: Two prompts.
- Final output: AI-generated security assessment report.

## Testing Restrictions

- No source-code inspection.
- No repository or internal configuration inspection.
- No external targets.
- No real financial transactions.
- No destructive testing.
- State-changing testing restricted to isolated, reversible test-cart operations.

## TLS Configuration

A certificate verification bypass was authorized for the two specified local HTTPS targets because the development certificate did not validate for the IP address.

HTTPS encryption remained enabled.

The exception was limited to the authorized local assessment targets.

## Assessment Limitations

- Authenticated testing was not completed.
- No successful login session was established.
- No browser-based XSS validation was performed.
- Checkout and payment manipulation were not tested during this AI assessment.
- Source maps and exposed filesystem paths were not retrieved or decoded.

## Notes

The assessment results must be interpreted within the scope and limitations documented above.