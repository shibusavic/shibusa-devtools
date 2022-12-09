using Shibusa.DevTools.Infrastructure.Schemas;

namespace Shibusa.DevTools.Infrastructure.Tests
{
    public class SchemaTests
    {
        [Fact]
        public void SortTablesByDependency()
        {
            var db = CreateDatabase();

            var sortedTables = db.GetTablesSortedByDependency();

            Assert.True(sortedTables.ElementAt(0).Equals(db.Tables.FirstOrDefault(t => t.Name == "A")), "A is not first table");
            Assert.True(sortedTables.ElementAt(1).Equals(db.Tables.FirstOrDefault(t => t.Name == "C")), "C is not second table");
            Assert.True(sortedTables.ElementAt(2).Equals(db.Tables.FirstOrDefault(t => t.Name == "B")), "B is not third table");
            Assert.True(sortedTables.ElementAt(3).Equals(db.Tables.FirstOrDefault(t => t.Name == "D")), "D is not fourth table");
            Assert.True(sortedTables.ElementAt(4).Equals(db.Tables.FirstOrDefault(t => t.Name == "E")), "E is not fifth table");

            Assert.True(true);
        }

        private static Database CreateDatabase()
        {
            List<Table> tables = new();
            List<View> views = new();
            List<Routine> routines = new();
            List<ForeignKey> foreignKeys = new();

            var tableA = CreateTable("dbo", "A");
            var tableB = CreateTable("dbo", "B", "FK", tableA.Name);
            var tableC = CreateTable("dbo", "C", "FK", tableA.Name);
            var tableD = CreateTable("dbo", "D", "FK", tableB.Name);
            var tableE = CreateTable("dbo", "E", "FK", tableD.Name);

            tables.Add(tableE);
            tables.Add(tableD);
            tables.Add(tableC);
            tables.Add(tableB);
            tables.Add(tableA);

            foreignKeys.Add(CreateForeignKey("dbo", "FK_A_B", tableA, tableB));
            foreignKeys.Add(CreateForeignKey("dbo", "FK_A_C", tableA, tableC));
            foreignKeys.Add(CreateForeignKey("dbo", "FK_B_D", tableB, tableD));
            foreignKeys.Add(CreateForeignKey("dbo", "FK_D_E", tableD, tableE));

            return new Database("Test", tables, foreignKeys, routines, views);
        }

        private static Table CreateTable(string schema, string name, string? fkColumn = null, string? fkTableName = null)
        {
            List<Column> columns = new()
            {
                new Column(schema, "Id", 1, null, false, "Int", 0, 0, true),
                new Column(schema, "Name", 2, null, false, "VarChar", 50, 0, false)
            };

            if (fkColumn != null && fkTableName != null)
            {
                columns.Add(new Column(schema, "FK", 3, null, false, "Int", 0, 0, false));
            }

            return new Table(schema, name, columns);
        }

        private static ForeignKey CreateForeignKey(string schema, string name, Table parentTable, Table childTable)
        {
            return new ForeignKey(schema, name, parentTable, "Id", childTable, "FK");
        }
    }
}