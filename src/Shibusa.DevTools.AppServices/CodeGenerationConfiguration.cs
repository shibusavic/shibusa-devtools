namespace Shibusa.DevTools.AppServices
{
    public struct CodeGenerationConfiguration
    {
        public CodeGenerationConfiguration()
        {
            DirectoryInfo = new DirectoryInfo(Path.GetTempPath());
            UseClasses = true;
            UsePropertyGetters = true;
            Namespace = "MyNamespace";
        }

        public CodeGenerationConfiguration(
            DirectoryInfo directoryInfo,
            bool useStructs,
            bool useClasses,
            bool useFields,
            bool includeDbAttributes,
            bool usePropertyGetters,
            bool usePropertySetters,
            bool generateConstructor,
            string codeNamespace)
        {
            DirectoryInfo = directoryInfo;
            UseStructs = useStructs;
            UseClasses = useClasses;
            UseFields = useFields;
            IncludeDbAttributes = includeDbAttributes;
            UsePropertyGetters = usePropertyGetters;
            UsePropertySetters = usePropertySetters;
            GenerateConstructor = generateConstructor;
            Namespace = codeNamespace;

            if (useStructs == useClasses) // if not the same, then only one of them is true.
            {
                if (useStructs)
                {
                    throw new ArgumentException("You cannot build a configuration that creates both classes and structs. Only one can be true.");
                }
                UseClasses = true; // default to creating classes.
            }

            if (useFields && (usePropertyGetters || usePropertySetters))
            {
                throw new ArgumentException("You cannot generate code that uses both fields and properties; you must choose.");
            }
        }

        public DirectoryInfo DirectoryInfo;

        public bool UseStructs;

        public bool UseClasses;

        public bool UseFields;

        public bool IncludeDbAttributes;

        public bool UsePropertyGetters;

        public bool UsePropertySetters;

        public bool GenerateConstructor;

        public string Namespace;

        public static CodeGenerationConfiguration CreateClassWithPropertiesConfiguration(
            DirectoryInfo directoryInfo,
            string codeNamespace,
            bool usePropertyGetters = true,
            bool usePropertySetters = false,
            bool includeDbAttributes = false,
            bool generateConstructor = false)
        {
            return new CodeGenerationConfiguration(directoryInfo,
                useStructs: false,
                useClasses: true,
                useFields: false,
                includeDbAttributes,
                usePropertyGetters,
                usePropertySetters,
                generateConstructor,
                codeNamespace);

        }

        public static CodeGenerationConfiguration CreateClassWithFieldsConfiguration(
            DirectoryInfo directoryInfo,
            string codeNamespace,
            bool includeDbAttributes = false,
            bool generateConstructor = false)
        {
            return new CodeGenerationConfiguration(directoryInfo,
                useStructs: false,
                useClasses: true,
                useFields: true,
                includeDbAttributes,
                usePropertyGetters: false,
                usePropertySetters: false,
                generateConstructor,
                codeNamespace);

        }

        public static CodeGenerationConfiguration CreateStructWithFieldsConfiguration(
            DirectoryInfo directoryInfo,
            string codeNamespace,
            bool includeDbAttributes = false,
            bool generateConstructor = false)
        {
            return new CodeGenerationConfiguration(directoryInfo,
                useStructs: true,
                useClasses: false,
                useFields: true,
                includeDbAttributes,
                usePropertyGetters: false,
                usePropertySetters: false,
                generateConstructor,
                codeNamespace);

        }

        public static CodeGenerationConfiguration CreateStructWithPropertiesConfiguration(
            DirectoryInfo directoryInfo,
            string codeNamespace,
            bool usePropertyGetters = true,
            bool usePropertySetters = false,
            bool includeDbAttributes = false,
            bool generateConstructor = false)
        {
            return new CodeGenerationConfiguration(directoryInfo,
                useStructs: true,
                useClasses: false,
                useFields: false,
                includeDbAttributes,
                usePropertyGetters,
                usePropertySetters,
                generateConstructor,
                codeNamespace);
        }
    }
}