using System.Collections.Generic;

namespace DynamicType
{
    public class TypeMetadata
    {
        public TypeMetadata(string assemblyName, string typeName, params FieldMetadata[] fields)
        {
            AssemblyName = assemblyName;
            TypeName = typeName;
            Fields = fields;
        }

        public string AssemblyName { get; }
        public IReadOnlyCollection<FieldMetadata> Fields { get; }
        public string TypeName { get; }
    }
}