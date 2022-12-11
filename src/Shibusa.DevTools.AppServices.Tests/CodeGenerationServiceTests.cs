using Moq;
using Shibusa.DevTools.Infrastructure.Abstractions;
using Shibusa.DevTools.Infrastructure.Schemas;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shibusa.DevTools.AppServices.Tests;

public class CodeGenerationServiceTests
{
    [Fact]
    public async Task ConvertTableToClass()
    {
        string schema = "public";

        int pos = 0;
        var table = new Table(schema, "test_table_class", new List<Column>()
        {
            new Column(schema, "Id", ++pos, null, false, "uuid", 0, 0, true),
            new Column(schema, "Name", ++pos, null, false, "text", 50, 0, false),
            new Column(schema, "Age", ++pos, null, true, "integer", 0, 0, false), // is nullable
        });

        var config = CodeGenerationConfiguration.CreateClassWithPropertiesConfiguration(
            new DirectoryInfo(Path.GetTempPath()), "CodeGenTest", true, false, false, false);

        var fileName = await CodeGenerationService.GenerateFromTableAsync(config, table);

        Assert.True(File.Exists(fileName));
    }

    [Fact]
    public async Task ConvertTableToStruct()
    {
        string schema = "public";

        int pos = 0;
        var table = new Table(schema, "test_table_struct", new List<Column>()
        {
            new Column(schema, "id", ++pos, null, false, "UUID", 0, 0, true),
            new Column(schema, "name", ++pos, null, false, "TEXT", 50, 0, false),
            new Column(schema, "age", ++pos, null, false, "INTEGER", 0, 0, false),
        });

        var config = CodeGenerationConfiguration.CreateStructWithFieldsConfiguration(
            new DirectoryInfo(Path.GetTempPath()), "CodeGenTest", true, false);

        var fileName = await CodeGenerationService.GenerateFromTableAsync(config, table);

        Assert.True(File.Exists(fileName));
    }

    [Fact]
    public async Task ClassCtor()
    {
        string schema = "public";

        int pos = 0;
        var table = new Table(schema, "test_table_class_CTOR", new List<Column>()
        {
            new Column(schema, "id", ++pos, null, false, "UUID", 0, 0, true),
            new Column(schema, "name", ++pos, null, false, "TEXT", 50, 0, false),
            new Column(schema, "age", ++pos, null, false, "DOUBLE PRECISION", 0, 0, false),
        });

        var config = CodeGenerationConfiguration.CreateClassWithPropertiesConfiguration(
            new DirectoryInfo(Path.GetTempPath()), "CodeGenTest", true, false, true);

        var fileName = await CodeGenerationService.GenerateFromTableAsync(config, table);

        Assert.True(File.Exists(fileName));
    }
}