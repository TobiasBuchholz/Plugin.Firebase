﻿using System.Collections.Generic;

namespace Plugin.Firebase.Firestore
{
    public sealed class SetOptions
    {
        public static SetOptions Merge() => new SetOptions(TypeMerge);   
        public static SetOptions MergeFieldPaths(IList<IList<string>> fieldPaths) => new SetOptions(TypeMergeFieldPaths, fieldPaths);
        public static SetOptions MergeFields(params string[] fields) => new SetOptions(TypeMergeFields, fields:fields);
        public static SetOptions MergeFields(IList<string> fields) => new SetOptions(TypeMergeFields, fields:fields);

        public const int TypeMerge = 0;
        public const int TypeMergeFieldPaths = 1;
        public const int TypeMergeFields = 2;

        public SetOptions(int type, IList<IList<string>> fieldPaths = null, IList<string> fields = null)
        {
            Type = type;
            FieldPaths = fieldPaths;
            Fields = fields;
        }
        
        public int Type { get; }
        public IList<IList<string>> FieldPaths { get; }
        public IList<string> Fields { get; }
    }
}