using AndroidRuntime = Android.Runtime;

namespace Plugin.Firebase.IntegrationTests;

internal static partial class IntegrationTestEnvironment
{
    private static partial string? GetPlatformConfigurationValue(string? androidSystemPropertyName)
    {
        return string.IsNullOrWhiteSpace(androidSystemPropertyName)
            ? null
            : GetAndroidSystemProperty(androidSystemPropertyName);
    }

    private static string? GetAndroidSystemProperty(string propertyName)
    {
        IntPtr? propertyValuePointer = null;

        try {
            var systemPropertiesClass = AndroidRuntime.JNIEnv.FindClass("android/os/SystemProperties");
            var getMethodId = AndroidRuntime.JNIEnv.GetStaticMethodID(
                systemPropertiesClass,
                "get",
                "(Ljava/lang/String;Ljava/lang/String;)Ljava/lang/String;");

            using var propertyNameValue = new Java.Lang.String(propertyName);
            using var defaultValue = new Java.Lang.String(string.Empty);
            propertyValuePointer = AndroidRuntime.JNIEnv.CallStaticObjectMethod(
                systemPropertiesClass,
                getMethodId,
                new AndroidRuntime.JValue(propertyNameValue),
                new AndroidRuntime.JValue(defaultValue));

            return AndroidRuntime.JNIEnv.GetString(
                propertyValuePointer.Value,
                AndroidRuntime.JniHandleOwnership.DoNotTransfer);
        } catch {
            return null;
        }
        finally {
            if(propertyValuePointer.HasValue && propertyValuePointer.Value != IntPtr.Zero) {
                AndroidRuntime.JNIEnv.DeleteLocalRef(propertyValuePointer.Value);
            }
        }
    }
}