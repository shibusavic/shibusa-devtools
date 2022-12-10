using Shibusa.DevTools.Infrastructure.Schemas;

namespace Shibusa.DevTools.Infrastructure.Abstractions
{
    public interface IDatabaseFactory
    {
        Task<Database> CreateAsync(string connectionString,
            bool includeTables = true,
            bool includeViews = true,
            bool includeRoutines = true,
            bool includeForeignKeys = true);
    }
}