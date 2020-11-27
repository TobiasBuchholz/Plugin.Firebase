using System;
using System.Linq;
using Plugin.Firebase.Android.Extensions;
using NativeFieldValue = Firebase.Firestore.FieldValue;

namespace Plugin.Firebase.Firestore
{
    public static class FieldValueExtensions
    {
        public static NativeFieldValue ToNative(this FieldValue @this)
        {
            switch(@this.Type) {
                case FieldValueType.ArrayUnion:
                    return NativeFieldValue.ArrayUnion(@this.Elements.Select(x => x.ToJavaObject()).ToArray());
                case FieldValueType.ArrayRemove:
                    return NativeFieldValue.ArrayRemove(@this.Elements.Select(x => x.ToJavaObject()).ToArray());
            }
            throw new ArgumentException($"Couldn't convert FieldValue to native because of unknown type: {@this.Type}");
        }
    }
}