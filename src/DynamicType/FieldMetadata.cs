namespace DynamicType
{
    public class FieldMetadata
    {
        public FieldMetadata(string fieldType, string fieldName)
        {
            FieldType = fieldType;
            FieldName = fieldName;
        }

        public string FieldName { get; }
        public string FieldType { get; }
    }
}