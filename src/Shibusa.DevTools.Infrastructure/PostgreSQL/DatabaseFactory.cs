using Dapper;
using Shibusa.DevTools.Infrastructure.Schemas;
using Npgsql;

namespace Shibusa.DevTools.Infrastructure.PostgreSQL
{
    /// <summary>
    /// A factory for constructing <see cref="Database"/> objects from a PostgreSQL database.
    /// </summary>
    public static class DatabaseFactory
    {
        /// <summary>
        /// Create an instance of the <see cref="Database"/> object representing the database of the provided connection string.
        /// </summary>
        /// <param name="connectionString">The database connection string.</param>
        /// <param name="includeTables">An indicator of whether to include tables.</param>
        /// <returns>A task representing the asyncronous operation; the task contains a
        /// <see cref="Database"/> instance.</returns>
        public static async Task<Database> CreateAsync(string connectionString,
            bool includeTables = true)
        //bool includeViews = true,
        //bool includeRoutines = true,
        //bool includeForeignKeys = true)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) { throw new ArgumentNullException(nameof(connectionString)); }

            var tables = includeTables ? await GetTablesAsync(connectionString).ConfigureAwait(false) : null;
            //var foreignKeys = includeForeignKeys ? await GetForeignKeysAsync(connectionString).ConfigureAwait(false) : null;
            //var views = includeViews ? await GetViewsAsync(connectionString).ConfigureAwait(false) : null;
            //var routines = includeRoutines ? await GetRoutinesAsync(connectionString).ConfigureAwait(false) : null;

            return new Database(GetDatabaseName(connectionString), tables, null, null, null);
        }

        private static string GetDatabaseName(string connectionString)
        {
            using var connection = new NpgsqlConnection(connectionString);
            return connection.Database;
        }

        private static async Task<IEnumerable<Table>> GetTablesAsync(string connectionString)
        {
            List<Table> tables = new();
            string tableSql = $"{GET_TABLES_SQL} WHERE table_schema = 'public' ORDER BY table_name";

            using var connection = new NpgsqlConnection(connectionString);

            var tableNames = await connection.QueryAsync<(string schema, string name)>(tableSql);

            foreach (var (schema, name) in tableNames)
            {
                string columnSql = $"{GET_COLUMNS_SQL} WHERE table_schema = @Schema AND table_name = @Name ORDER BY ordinal_position";

                var columns = await connection.QueryAsync<Column>(columnSql,
                    new
                    {
                        Schema = schema,
                        Name = name
                    });

                tables.Add(new Table(schema, name, columns));
            }
            return tables;
        }

        //private static async Task<IEnumerable<ForeignKey>> GetForeignKeysAsync(string connectionString)
        //{
        //    List<ForeignKey> foreignKeys = new();

        //    string fkSql = $"{GET_FOREIGN_KEYS_SQL} order by [Schema], TableName";

        //    using var connection = new NpgsqlConnection(connectionString);

        //    var fks = await connection.QueryAsync<ForeignKeyDto>(fkSql);

        //    foreach (var fk in fks)
        //    {
        //        var table = await GetTableAsync(connectionString, fk.Schema, fk.TableName);
        //        if (table != null)
        //        {
        //            var refTable = await GetTableAsync(connectionString, fk.ReferenceSchema, fk.ReferenceTableName);
        //            foreignKeys.Add(new ForeignKey(table.Schema, fk.Name, refTable, fk.ReferenceColumnName, table, fk.ReferenceColumnName));
        //        }
        //    }

        //    return foreignKeys;
        //}

        //private static async Task<IEnumerable<View>> GetViewsAsync(string connectionString)
        //{
        //    string viewSql = $"{GET_VIEWS_SQL} ORDER BY TABLE_SCHEMA, TABLE_NAME";
        //    using var connection = new NpgsqlConnection(connectionString);
        //    return await connection.QueryAsync<View>(viewSql);
        //}

        //private static async Task<IEnumerable<Routine>> GetRoutinesAsync(string connectionString)
        //{
        //    string routineSql = $"{GET_ROUTINES_SQL} ORDER BY ROUTINE_SCHEMA, ROUTINE_NAME";
        //    using var connection = new NpgsqlConnection(connectionString);
        //    return await connection.QueryAsync<Routine>(routineSql);
        //}

        private static async Task<Table?> GetTableAsync(string connectionString, string schema, string name)
        {
            using var connection = new NpgsqlConnection(connectionString);

            string columnSql = $"{GET_COLUMNS_SQL} WHERE table_schema = @Schema AND table_name = @Name ORDER BY ordinal_position";

            var columns = await connection.QueryAsync<Column>(columnSql, new
            {
                Schema = schema,
                Name = name
            });

            if (columns.Any())
            {
                return new Table(schema, name, columns);
            }

            return null;
        }

        private const string GET_TABLES_SQL = @"
SELECT table_schema AS Schema, 
table_name AS Name
FROM information_schema.tables WHERE table_schema = 'public'
";

        private const string GET_COLUMNS_SQL = @"
SELECT
table_schema AS Schema,
column_name AS Name,
ordinal_position AS OrdinalPosition,
column_default AS ColumnDefault,
CASE is_nullable
WHEN 'YES' THEN true
ELSE false
END AS IsNullable,
data_type AS DataType,
character_maximum_length as MaxLength,
numeric_precision AS NumericPrecision
FROM information_schema.columns
";

        //        private const string GET_FOREIGN_KEYS_SQL = @"
        //SELECT
        //f.name AS [NAME],
        //SCHEMA_NAME(f.SCHEMA_ID) AS [SCHEMA],
        //OBJECT_NAME(f.parent_object_id) AS TableName,
        //COL_NAME(fc.parent_object_id,fc.parent_column_id) AS ColumnName,
        //SCHEMA_NAME(o.SCHEMA_ID) AS ReferenceSchema,
        //OBJECT_NAME (f.referenced_object_id) AS ReferenceTableName,
        //COL_NAME(fc.referenced_object_id,fc.referenced_column_id) AS ReferenceColumnName
        //FROM sys.foreign_keys AS f
        //INNER JOIN sys.foreign_key_columns AS fc ON f.OBJECT_ID = fc.constraint_object_id
        //INNER JOIN sys.objects AS o ON o.OBJECT_ID = fc.referenced_object_id
        //";

        //        private const string GET_VIEWS_SQL = @"
        //SELECT
        //TABLE_SCHEMA AS [SCHEMA],
        //TABLE_NAME AS [NAME],
        //VIEW_DEFINITION AS [DEFINITION]
        //FROM INFORMATION_SCHEMA.VIEWS
        //";

        //        private const string GET_ROUTINES_SQL = @"
        //SELECT
        //[ROUTINE_SCHEMA] AS [SCHEMA],
        //[ROUTINE_NAME] AS [NAME],
        //[ROUTINE_DEFINITION] AS [DEFINITION],
        //[ROUTINE_TYPE] AS [ROUTINETYPE]
        //FROM [INFORMATION_SCHEMA].[ROUTINES]
        //";
    }
}
