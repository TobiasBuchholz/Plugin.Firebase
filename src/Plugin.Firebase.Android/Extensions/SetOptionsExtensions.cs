using System;
using System.Linq;
using Firebase.Firestore;
using NativeSetOptions = Firebase.Firestore.SetOptions;
using SetOptions = Plugin.Firebase.Abstractions.Firestore.SetOptions;

namespace Plugin.Firebase.Android.Extensions
{
    public static class SetOptionsExtensions
    {
        public static NativeSetOptions ToNative(this SetOptions options)
        {
            switch(options.Type) {
                case SetOptions.TypeMerge:
                    return NativeSetOptions.Merge();
                case SetOptions.TypeMergeFieldPaths:
                    return NativeSetOptions.MergeFieldPaths(options.FieldPaths.Select(x => FieldPath.Of(x.ToArray())).ToList());
                case SetOptions.TypeMergeFields:
                    return NativeSetOptions.MergeFields(options.Fields);
                default:
                    throw new ArgumentException($"SetOptions type {options.Type} is not supported.");
            }
        }
    }
}