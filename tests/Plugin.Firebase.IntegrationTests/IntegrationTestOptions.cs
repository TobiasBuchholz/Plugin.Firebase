namespace Plugin.Firebase.IntegrationTests;

internal static class IntegrationTestOptions
{
    public const string BackendEnvironmentVariableName = "PLUGIN_FIREBASE_TEST_BACKEND";
    public const string BackendAndroidSystemPropertyName = "debug.pluginfirebase.backend";

    public const string UseVisualRunnerEnvironmentVariableName = "PLUGIN_FIREBASE_USE_VISUAL_RUNNER";
    public const string UseVisualRunnerAndroidSystemPropertyName = "debug.pluginfirebase.visual.use";

    public const string RunAppCheckTokenTestsEnvironmentVariableName = "PLUGIN_FIREBASE_RUN_APPCHECK_TOKEN_TESTS";
    public const string RunFcmTokenTestsEnvironmentVariableName = "PLUGIN_FIREBASE_RUN_FCM_TOKEN_TESTS";
    public const string RunFcmDeliveryTestsEnvironmentVariableName = "PLUGIN_FIREBASE_RUN_FCM_DELIVERY_TESTS";
    public const string ForceCrashlyticsCrashEnvironmentVariableName = "PLUGIN_FIREBASE_FORCE_CRASHLYTICS_CRASH";
    public const string ExpectPreviousCrashEnvironmentVariableName = "PLUGIN_FIREBASE_EXPECT_PREVIOUS_CRASH";
    public const string RunInstallationsDeleteTestsEnvironmentVariableName =
        "PLUGIN_FIREBASE_RUN_INSTALLATIONS_DELETE_TESTS";

    public const string RunPhoneAuthTestsEnvironmentVariableName = "PLUGIN_FIREBASE_RUN_PHONE_AUTH_TESTS";
    public const string RunPhoneAuthTestsAndroidSystemPropertyName = "debug.pluginfirebase.phone.run";
    public const string PhoneAuthNumberEnvironmentVariableName = "PLUGIN_FIREBASE_PHONE_AUTH_NUMBER";
    public const string PhoneAuthNumberAndroidSystemPropertyName = "debug.pluginfirebase.phone.number";
    public const string PhoneAuthCodeEnvironmentVariableName = "PLUGIN_FIREBASE_PHONE_AUTH_CODE";
    public const string PhoneAuthCodeAndroidSystemPropertyName = "debug.pluginfirebase.phone.code";
    public const string PhoneAuthVerificationIdEnvironmentVariableName =
        "PLUGIN_FIREBASE_PHONE_AUTH_VERIFICATION_ID";
    public const string PhoneAuthVerificationIdAndroidSystemPropertyName =
        "debug.pluginfirebase.phone.verification_id";

    public const string UseAuthEmulatorEnvironmentVariableName = "PLUGIN_FIREBASE_USE_AUTH_EMULATOR";
    public const string UseAuthEmulatorAndroidSystemPropertyName = "debug.pluginfirebase.auth.use";
    public const string AuthEmulatorHostEnvironmentVariableName = "PLUGIN_FIREBASE_AUTH_EMULATOR_HOST";
    public const string AuthEmulatorHostAndroidSystemPropertyName = "debug.pluginfirebase.auth.host";
    public const string AuthEmulatorPortEnvironmentVariableName = "PLUGIN_FIREBASE_AUTH_EMULATOR_PORT";
    public const string AuthEmulatorPortAndroidSystemPropertyName = "debug.pluginfirebase.auth.port";

    public const string UseFirestoreEmulatorEnvironmentVariableName = "PLUGIN_FIREBASE_USE_FIRESTORE_EMULATOR";
    public const string UseFirestoreEmulatorAndroidSystemPropertyName = "debug.pluginfirebase.firestore.use";
    public const string FirestoreEmulatorHostEnvironmentVariableName = "PLUGIN_FIREBASE_FIRESTORE_EMULATOR_HOST";
    public const string FirestoreEmulatorHostAndroidSystemPropertyName = "debug.pluginfirebase.firestore.host";
    public const string FirestoreEmulatorPortEnvironmentVariableName = "PLUGIN_FIREBASE_FIRESTORE_EMULATOR_PORT";
    public const string FirestoreEmulatorPortAndroidSystemPropertyName = "debug.pluginfirebase.firestore.port";

    public const string UseFunctionsEmulatorEnvironmentVariableName = "PLUGIN_FIREBASE_USE_FUNCTIONS_EMULATOR";
    public const string UseFunctionsEmulatorAndroidSystemPropertyName = "debug.pluginfirebase.functions.use";
    public const string FunctionsEmulatorHostEnvironmentVariableName = "PLUGIN_FIREBASE_FUNCTIONS_EMULATOR_HOST";
    public const string FunctionsEmulatorHostAndroidSystemPropertyName = "debug.pluginfirebase.functions.host";
    public const string FunctionsEmulatorPortEnvironmentVariableName = "PLUGIN_FIREBASE_FUNCTIONS_EMULATOR_PORT";
    public const string FunctionsEmulatorPortAndroidSystemPropertyName = "debug.pluginfirebase.functions.port";

    public const string UseStorageEmulatorEnvironmentVariableName = "PLUGIN_FIREBASE_USE_STORAGE_EMULATOR";
    public const string UseStorageEmulatorAndroidSystemPropertyName = "debug.pluginfirebase.storage.use";
    public const string StorageEmulatorHostEnvironmentVariableName = "PLUGIN_FIREBASE_STORAGE_EMULATOR_HOST";
    public const string StorageEmulatorHostAndroidSystemPropertyName = "debug.pluginfirebase.storage.host";
    public const string StorageEmulatorPortEnvironmentVariableName = "PLUGIN_FIREBASE_STORAGE_EMULATOR_PORT";
    public const string StorageEmulatorPortAndroidSystemPropertyName = "debug.pluginfirebase.storage.port";
}