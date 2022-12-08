using System.ComponentModel;

/// <summary>
/// Represents the available database engines.
/// </summary>
public enum DatabaseEngine
{
    [Description("None")]
    None = 0,
    [Description("PostgreSQL")]
    Postgres,
    //[Description("MS SQL Server")]
    //SqlServer,
    //[Description("MySQL")]
    //MySql,
    //[Description("SQLite")]
    //Sqlite
}
