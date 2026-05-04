# Integration Test Harness

`Plugin.Firebase.IntegrationTests` is a MAUI/xUnit device test app. The default backend is the Firebase Local Emulator Suite for Auth, Firestore, Functions, and Storage. Real Firebase project tests must stay opt-in.

## Fact Attributes

- Use `Fact` for tests that run on both emulator and real backends.
- Use `EmulatorBackendFact` / `EmulatorBackendTheory` when the test requires local emulators.
- Use `RealFirebaseFact` or `RealFirebaseOptInFact` when Firebase has no local emulator or the test needs a configured real project.
- Use `AndroidFact`, `IosFact`, or `IosDeviceFact` for platform-only behavior instead of returning early from the test body.
- Use `OptInFact` for destructive, paid, external-delivery, or manually coordinated tests.
- Use `IntegrationTestCase` only when a plain xUnit attribute needs explicit backend/platform/opt-in metadata.

## Coverage Metadata

- Put `IntegrationTestFixture(IntegrationTestPackage.X)` on the root declaration for every fixture that maps to package acceptance coverage.
- Use `IntegrationTestCoverageIgnore` only for harness-only fixtures, and include the reason in the attribute.
- Keep `ACCEPTANCE_COVERAGE.md` as the source of package coverage expectations. Dynamic Links is intentionally excluded.
- Run `scripts/check-integration-coverage.rb` after adding, renaming, or moving fixtures.

## Resource Cleanup

- Prefer explicit scopes over fixture-wide cleanup when a test creates backend state.
- Use `AuthTestUserScope` for temporary Auth users.
- Use `FirestoreTestCollectionScope` for unique Firestore collections.
- Use `StorageTestPathScope` for temporary Storage files.
- Use `IntegrationTestResourceScope` when one test owns multiple async cleanup resources; add resources as soon as they are created.
- Cleanup should log failures with `TestLog` and avoid deleting shared seed data.
- Shared seeded Auth users, custom-claims users, and destructive opt-in flows must stay explicit and should not be hidden inside broad cleanup.
- Fixture-level Auth cleanup is a defensive fallback. New temporary Auth flows should own their user through `AuthTestUserScope`.

## C# Test Layout

- Keep `PackageFixture.cs` for lifecycle, setup, shared resource accessors, and package-level state.
- Keep `PackageFixture.Behavior.cs` files focused on test methods and visible Firebase API calls.
- Put DTOs and Firestore documents in `PackagePayloads.cs` files, data construction in `PackageFactories.cs`, and reusable checks in `PackageAssertions.cs`.
- Keep fixture files under roughly 200 lines unless a behavior area genuinely needs more room.

## Probes and Helpers

- Use `CallbackProbe<T>` for callback/listener completion instead of ad hoc `TaskCompletionSource<T>`.
- Use `EventProbe<TEventArgs>` for .NET events that should unsubscribe reliably at the end of the test.
- Keep timeout names explicit at the wait site so failures identify the operation.
- Put repeated wire field names beside the payload/model type they belong to; leave one-off field names inline when that is clearer.
- Helper extraction must preserve test behavior, xUnit skip attributes, and coverage metadata.

## Seed Data

- Auth emulator seed data lives in `tests/cloud-functions/scripts/seed-auth-emulator.js`.
- Functions emulator behavior lives in `tests/cloud-functions/functions/src/index.ts`.
- Storage tests create emulator seed files through the test harness and should only assume real-backend seed files documented in `docs/BUILDING.md`.

## Adding Coverage

- Keep tests close to native Firebase behavior and avoid app-specific policy.
- Add new public API coverage to `ACCEPTANCE_COVERAGE.md`.
- Keep real-backend requirements documented in `docs/BUILDING.md`.
- Prefer `WaitForTestAsync` and `EventuallyAsync` for device callbacks so failures name the operation that timed out.
- Keep fixture files behavior-focused. For broad fixtures, split files by operation family such as credentials, listeners, payload serialization, dictionary values, nullability, uploads, downloads, metadata, and cleanup.
- Move reusable Firestore document/payload types into internal files under `Firestore/` instead of nesting them in arrange/act/assert flows.

## Running Locally

- Use `scripts/check-integration-environment.sh android|ios` before a device run to verify CLIs, built app output, function build output, emulator ports, and target availability.
- `scripts/run-integration-emulators.sh android|ios` calls preflight automatically. Set `SKIP_INTEGRATION_PREFLIGHT=1` only for CI edge cases where another step has already guaranteed the environment.
- GitHub summaries from `scripts/write-xunit-github-summary.rb` include totals, failures, skips, slowest tests, and recent `[TEST START]` breadcrumbs when logs are present.
