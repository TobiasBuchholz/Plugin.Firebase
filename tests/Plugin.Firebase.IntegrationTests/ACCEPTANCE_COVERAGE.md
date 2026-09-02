# Integration Acceptance Coverage

`Plugin.Firebase.IntegrationTests` is the acceptance suite for active packages. Dynamic Links is intentionally excluded.

Use this file as the checklist when adding or changing public mobile APIs: every active public surface should be covered automatically, explicitly gated behind a real-backend or opt-in attribute, or documented as not locally assertable.

Run `scripts/check-integration-coverage.rb` after adding or renaming fixtures. The script verifies that every fixture is annotated with `IntegrationTestFixture` metadata and maps to a package listed in the table below. Harness-only fixtures must opt out with `IntegrationTestCoverageIgnore` and a reason.

| Package | Default emulator gate | Real backend | Opt-in/manual | Notes |
|---|---:|---:|---:|---|
| Analytics | No | Yes | No | Verifies SDK acceptance and observable local state; Firebase Console ingestion is not asserted. |
| App Check | Partial | Optional | `PLUGIN_FIREBASE_RUN_APPCHECK_TOKEN_TESTS` | Disabled/debug/provider behavior is automatic; token enforcement requires a real project. |
| Auth | Yes | Partial | `PLUGIN_FIREBASE_RUN_PHONE_AUTH_TESTS` | Email/password, anonymous, custom tokens, email links, metadata, and claims are covered. Phone auth requires external credentials. |
| Bundled initializer | Yes | Yes | No | Verifies singleton access and dispose/reacquire behavior without reconfiguring initialized native SDKs. |
| Cloud Messaging | Partial | Optional | `PLUGIN_FIREBASE_RUN_FCM_TOKEN_TESTS`, `PLUGIN_FIREBASE_RUN_FCM_DELIVERY_TESTS` | Synthetic events are automatic. Token and push delivery require a real project; iOS delivery requires a physical device with APNs. |
| Crashlytics | Partial | Partial | `PLUGIN_FIREBASE_FORCE_CRASHLYTICS_CRASH`, `PLUGIN_FIREBASE_EXPECT_PREVIOUS_CRASH` | Non-crash APIs are automatic. Previous-crash detection needs a destructive two-run flow. |
| Firestore | Yes | Yes | No | Emulator-backed PR gate covers document, query, conversion, nullability, listener, lifecycle, and offline behavior. |
| Functions | Yes | Yes | No | Real backend requires deploying `tests/cloud-functions/functions`. |
| Installations | Partial | Yes | `PLUGIN_FIREBASE_RUN_INSTALLATIONS_DELETE_TESTS` | Delete is opt-in because it resets the shared installation identity. |
| Performance Monitoring | Partial | Yes | No | Verifies SDK acceptance and local state; Firebase Console ingestion is not asserted. |
| Remote Config | No | Yes | No | Requires published real-project parameters. |
| Storage | Yes | Yes | No | Covers root/parent references, pagination tokens, metadata and timestamps, transfers, and success/failure snapshot nullability. Real backend requires bucket seed files and permissive test rules. |

## Expected Fixture Metadata

| Metadata package | Acceptance coverage row |
|---|---|
| `IntegrationTestPackage.Analytics` | Analytics |
| `IntegrationTestPackage.AppCheck` | App Check |
| `IntegrationTestPackage.Auth` | Auth |
| `IntegrationTestPackage.Bundled` | Bundled initializer |
| `IntegrationTestPackage.CloudMessaging` | Cloud Messaging |
| `IntegrationTestPackage.Crashlytics` | Crashlytics |
| `IntegrationTestPackage.Firestore` | Firestore |
| `IntegrationTestPackage.Functions` | Functions |
| `IntegrationTestPackage.Installations` | Installations |
| `IntegrationTestPackage.PerformanceMonitoring` | Performance Monitoring |
| `IntegrationTestPackage.RemoteConfig` | Remote Config |
| `IntegrationTestPackage.Storage` | Storage |

## Harness Rules

- Prefer `Fact`, `EmulatorBackendFact`, `RealFirebaseFact`, `OptInFact`, or `RealFirebaseOptInFact` over runtime skips. Runtime skips can be reported as failures by device runners.
- Put `IntegrationTestFixture(IntegrationTestPackage.X)` on each package fixture root. Use the backend/platform/opt-in fact attributes for method-level metadata, and add `IntegrationTestCase` only when a plain xUnit attribute needs explicit metadata.
- Use `IntegrationTestData` for unique resource names and opt-in configuration values.
- Use `WaitForTestAsync` or `EventuallyAsync` for asynchronous device callbacks so timeout failures include the operation being awaited.
- Keep destructive, paid, external-delivery, or console-ingestion checks behind opt-in attributes.
