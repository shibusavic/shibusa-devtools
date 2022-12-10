using Shibusa.DevTools.Infrastructure.Abstractions;
using Shibusa.DevTools.Infrastructure.Schemas;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shibusa.DevTools.AppServices;

public class CodeGenerationService
{
    private readonly IDatabaseFactory databaseFactory;

    public CodeGenerationService(IDatabaseFactory databaseFactory)
    {
        this.databaseFactory = databaseFactory;
    }

    public async Task<string> GenerateFromTableAsync(CodeGenerationConfiguration configuration, Table table)
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
                            { "field-name", ConvertDbNameToCSharpPublicName(col.Value.Name) },
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
                            { "field-name", ConvertDbNameToCSharpPublicName(col.Value.Name) }
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
                            { "prop-name", ConvertDbNameToCSharpPublicName(col.Value.Name) },
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
                            { "prop-name", ConvertDbNameToCSharpPublicName(col.Value.Name) },
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
            { "object-name", ConvertDbNameToCSharpPublicName(table.Name)},
            { "object-constructor", GenerateConstructor(table)},
            { "object-body", string.Join(Environment.NewLine, collection)},
            { "table-attribute", ""}
        };

        if (configuration.IncludeDbAttributes)
        {
            templateKeys["table-attribute"] = TableAttribute;
            templateKeys.Add("table-name", table.Name);
            templateKeys.Add("table-schema", table.Schema);
        }

        var fileName = Path.Combine(configuration.DirectoryInfo.FullName, $"{ConvertDbNameToCSharpPublicName(table.Name)}.cs");

        await File.WriteAllTextAsync(fileName, Transformations.TransformRawText.TransformPounds(ObjectTemplate,
            templateKeys, true));

        return fileName;
    }

    private string GenerateConstructor(Table table)
    {
        return Transformations.TransformRawText.TransformPounds(CtorTemplate, new Dictionary<string, string>()
        {
            { "parm-list", string.Join($",{Environment.NewLine}", GenerateParameterList(table))},
            { "prop-setter", string.Join($"{Environment.NewLine}", GenerateParameterSetList(table))},
            { "object-name", ConvertDbNameToCSharpPublicName(table.Name)},
        }, true);
    }

    private IEnumerable<string> GenerateParameterList(Table table)
    {
        foreach (var col in table.Columns)
        {
            yield return $"{GetTabs(3)}{ConvertColumnTypeToCSharpType(col.Value.DataType,
                col.Value.IsNullable)} {ConvertDbNameToCSharpPrivateName(col.Value.Name)}";
        }
    }

    private IEnumerable<string> GenerateParameterSetList(Table table)
    {
        foreach (var col in table.Columns)
        {
            yield return $"{GetTabs(3)}{ConvertDbNameToCSharpPublicName(col.Value.Name)} = {ConvertDbNameToCSharpPrivateName(col.Value.Name)};";
        }
    }

    private string ConvertColumnTypeToCSharpType(string type, bool isNullable)
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

    private string ConvertDbNameToCSharpPublicName(string name)
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

    private string ConvertDbNameToCSharpPrivateName(string name)
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


    private const string ObjectTemplate = @"
using namespace #namespace#
{
#table-attribute#
    internal #object-type# #object-name#
    {
#object-constructor#
#object-body#
    }
}
";

    private string CtorTemplate = $@"{GetTabs(2)}public #object-name#(
#parm-list#)
{GetTabs(2)}{{
#prop-setter#
{GetTabs(2)}}}
";

    private string TableAttribute => $"{GetTabs(1)}[Table(name: \"#table-name#\", Schema = \"#table-schema#\")]";

    private string PropertyTemplateWithAttribute => $"{GetTabs(2)}[Column(\"#col-name#\", Order = #col-order#, TypeName = \"#col-type-name#\")]{Environment.NewLine}{PropertyTemplate}";

    private string PropertyTemplate => $"{GetTabs(2)}public #prop-type# #prop-name# {{ #prop-get# #prop-set# }}";

    private string FieldTemplateWithAttribute => $"{GetTabs(2)}[Column(\"#col-name#\", Order = #col-order#, TypeName = \"#col-type-name#\")]{Environment.NewLine}{FieldTemplate}";

    private string FieldTemplate => $"{GetTabs(2)}public #field-type# #field-name#;";

    private const int SizeOfTabs = 4;

    private static string GetTabs(int numberOfTabs = 1)
    {
        return new string(' ', numberOfTabs * SizeOfTabs);
    }
}
