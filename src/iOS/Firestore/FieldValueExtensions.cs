using System;
using System.Linq;
using Plugin.Firebase.Firestore;
using Plugin.Firebase.iOS.Extensions;
using NativeFieldValue = Firebase.CloudFirestore.FieldValue;

namespace Ablaze.UI.iOS.Plugin.Firebase.Firestore
{
    public static class FieldValueExtensions
    {
        public static NativeFieldValue ToNative(this FieldValue @this)
        {
            switch(@this.Type) {
                case FieldValueType.ArrayUnion:
                    return NativeFieldValue.FromArrayUnion(@this.Elements.Select(x => x.ToNSObject()).ToArray());
                case FieldValueType.ArrayRemove:
                    return NativeFieldValue.FromArrayRemove(@this.Elements.Select(x => x.ToNSObject()).ToArray());
                case FieldValueType.IntegerIncrement:
                    return NativeFieldValue.FromIntegerIncrement((long) @this.IncrementValue);
                case FieldValueType.DoubleIncrement:
                    return NativeFieldValue.FromDoubleIncrement(@this.IncrementValue);
            }
            throw new ArgumentException($"Couldn't convert FieldValue to native because of unknown type: {@this.Type}");
        }
    }
}