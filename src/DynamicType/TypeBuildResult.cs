using System;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace DynamicType
{
    public class TypeBuildResult
    {
        private TypeBuildResult(bool succeeded, Assembly assembly = null, MetadataReference reference = null, Type type = null)
        {
            Success = succeeded;
            Type = type;
            Assembly = assembly;
            Reference = reference;
        }

        public Assembly Assembly { get; }
        public MetadataReference Reference { get; }
        public bool Success { get; }
        public Type Type { get; }

        public static TypeBuildResult Failed() => new TypeBuildResult(false);

        public static TypeBuildResult Succeeded(Assembly assembly, MetadataReference reference, Type type) => new TypeBuildResult(true, assembly, reference, type);
    }
}