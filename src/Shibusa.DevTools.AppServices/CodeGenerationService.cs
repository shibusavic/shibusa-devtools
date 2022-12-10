using Shibusa.DevTools.Infrastructure.Schemas;

namespace Shibusa.DevTools.AppServices;

public static class CodeGenerationService
{
    public static async Task<string> GenerateFromTableAsync(CodeGenerationConfiguration configuration, Table table)
    {
        List<string> collection = new();

        foreach (var col in table.Columns)
        {
            if (configuration.UseFields)
            {
                if (configuration.IncludeDbAttributes)
                {
                    collection.Add(Transformations.TransformRawText.TransformPounds(FieldTemplateWithAttribute,
                        new Dictionary<string, string>() {
                            { "field-type", ConvertColumnTypeToCSharpType(col.Value.DataType, col.Value.IsNullable)},
                            { "field-name", ConvertToCsPublicName(col.Value.Name) },
                            { "col-name", col.Value.Name},
                            { "col-order", col.Value.OrdinalPosition.ToString()},
                            { "col-type-name", col.Value.DataType }
                         }, true));
                }
                else
                {
                    collection.Add(Transformations.TransformRawText.TransformPounds(FieldTemplate,
                        new Dictionary<string, string>() {
                            { "field-type", ConvertColumnTypeToCSharpType(col.Value.DataType, col.Value.IsNullable)},
                            { "field-name", ConvertToCsPublicName(col.Value.Name) }
                        }, true));
                }

            }
            else
            {
                if (configuration.IncludeDbAttributes)
                {
                    collection.Add(Transformations.TransformRawText.TransformPounds(PropertyTemplateWithAttribute,
                        new Dictionary<string, string>
                        {
                            { "prop-type", ConvertColumnTypeToCSharpType(col.Value.DataType, col.Value.IsNullable) },
                            { "prop-name", ConvertToCsPublicName(col.Value.Name) },
                            { "prop-get", configuration.UsePropertyGetters ? "get;" : "" },
                            { "prop-set", configuration.UsePropertySetters ? "set;" : "" },
                            { "col-name", col.Value.Name},
                            { "col-order", col.Value.OrdinalPosition.ToString()},
                            { "col-type-name", col.Value.DataType }
                        }, true));
                }
                else
                {
                    collection.Add(Transformations.TransformRawText.TransformPounds(PropertyTemplate,
                        new Dictionary<string, string>
                        {
                            { "prop-type", ConvertColumnTypeToCSharpType(col.Value.DataType, col.Value.IsNullable) },
                            { "prop-name", ConvertToCsPublicName(col.Value.Name) },
                            { "prop-get", configuration.UsePropertyGetters ? "get;" : "" },
                            { "prop-set", configuration.UsePropertySetters ? "set;" : "" }
                        }, true));
                }
            }
        }

        var templateKeys = new Dictionary<string, string>()
        {
            { "namespace", configuration.Namespace },
            { "object-type", configuration.UseStructs ? "struct" : "class"},
            { "object-name", ConvertToCsPublicName(table.Name)},
            { "object-constructor", GenerateConstructor(table)},
            { "object-body", string.Join(Environment.NewLine, collection)},
            { "object-usings", configuration.IncludeDbAttributes
                ? "using System.ComponentModel.DataAnnotations.Schema;" : "" },
            { "table-attribute", configuration.IncludeDbAttributes ? TableAttribute : ""},
            { "table-name", table.Name},
            { "table-schema", table.Schema}
        };

        var fileName = Path.Combine(configuration.DirectoryInfo.FullName, $"{ConvertToCsPublicName(table.Name)}.cs");

        await File.WriteAllTextAsync(fileName, Transformations.TransformRawText.TransformPounds(ObjectTemplate,
            templateKeys, true));

        return fileName;
    }

    private static string GenerateConstructor(Table table)
    {
        return Transformations.TransformRawText.TransformPounds(CtorTemplate, new Dictionary<string, string>()
        {
            { "parm-list", string.Join($",{Environment.NewLine}", GenerateParameterList(table))},
            { "prop-setter", string.Join($"{Environment.NewLine}", GenerateParameterSetList(table))},
            { "object-name", ConvertToCsPublicName(table.Name)},
        }, true);
    }

    private static IEnumerable<string> GenerateParameterList(Table table)
    {
        foreach (var col in table.Columns)
        {
            yield return $"{GetTabs(3)}{ConvertColumnTypeToCSharpType(col.Value.DataType,
                col.Value.IsNullable)} {ConvertToCsPrivateName(col.Value.Name)}";
        }
    }

    private static IEnumerable<string> GenerateParameterSetList(Table table)
    {
        foreach (var col in table.Columns)
        {
            yield return $"{GetTabs(3)}{ConvertToCsPublicName(col.Value.Name)} = {ConvertToCsPrivateName(col.Value.Name)};";
        }
    }

    private static string ConvertColumnTypeToCSharpType(string type, bool isNullable)
    {
        string convertedType = type.ToLowerInvariant() switch
        {
            string t when t is "bigint" or "int8" => "int",
            string t when t is "bit" or "boolean" or "bool" => "bool",
            string t when t is "character" or "character varying" or "text" => "string",
            string t when t is "date" or "time" or "timestamp" => "DateTime",
            string t when t is "double precision" => "double",
            string t when t is "integer" or "serial" => "int",
            string t when t is "money" or "numeric" => "decimal",
            string t when t is "smallint" or "smallserial" => "short",
            string t when t is "real" => "float",
            string t when t is "bytea" => "byte[]",
            string t when t is "uuid" => "Guid",
            _ => "object"
        };

        return $"{convertedType}{(isNullable ? "?" : "")}";
    }

    private static string ConvertToCsPublicName(string name)
    {
        List<char> chars = new();

        bool nextCharIsUpper = true;
        for (int c = 0; c < name.Length; c++)
        {
            if (!char.IsLetter(name[c]))
            {
                nextCharIsUpper = true;
                continue;
            }

            if (nextCharIsUpper)
            {
                chars.Add(char.ToUpper(name[c]));
                nextCharIsUpper = false;
            }
            else
            {
                chars.Add(char.ToLower(name[c]));
            }
        }

        return new string(chars.ToArray());
    }

    private static string ConvertToCsPrivateName(string name)
    {
        List<char> chars = new();

        bool nextCharIsUpper = false;
        for (int c = 0; c < name.Length; c++)
        {
            if (!char.IsLetter(name[c]) && c > 0)
            {
                nextCharIsUpper = true;
                continue;
            }

            if (nextCharIsUpper)
            {
                chars.Add(char.ToUpper(name[c]));
                nextCharIsUpper = false;
            }
            else
            {
                chars.Add(char.ToLower(name[c]));
            }
        }

        return new string(chars.ToArray());
    }


    private const string ObjectTemplate = @"#object-usings#
namespace #namespace#
{
#table-attribute#
    internal #object-type# #object-name#
    {
#object-constructor#
#object-body#
    }
}
";

    private static readonly string CtorTemplate = $@"{GetTabs(2)}public #object-name#(
#parm-list#)
{GetTabs(2)}{{
#prop-setter#
{GetTabs(2)}}}
";

    private static string TableAttribute => $"{GetTabs(1)}[Table(name: \"#table-name#\", Schema = \"#table-schema#\")]";

    private static string PropertyTemplateWithAttribute => $"{Environment.NewLine}{GetTabs(2)}[Column(\"#col-name#\", Order = #col-order#, TypeName = \"#col-type-name#\")]{PropertyTemplate}";

    private static string PropertyTemplate => $"{Environment.NewLine}{GetTabs(2)}public #prop-type# #prop-name# {{ #prop-get# #prop-set# }}";

    private static string FieldTemplateWithAttribute => $"{Environment.NewLine}{GetTabs(2)}[Column(\"#col-name#\", Order = #col-order#, TypeName = \"#col-type-name#\")]{FieldTemplate}";

    private static string FieldTemplate => $"{Environment.NewLine}{GetTabs(2)}public #field-type# #field-name#;";

    private const int SizeOfTabs = 4;

    private static string GetTabs(int numberOfTabs = 1)
    {
        return new string(' ', numberOfTabs * SizeOfTabs);
    }
}
