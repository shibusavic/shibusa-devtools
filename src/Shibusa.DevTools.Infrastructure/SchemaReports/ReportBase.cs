using Shibusa.DevTools.Infrastructure.Schemas;
using System.Text;

namespace Shibusa.DevTools.Infrastructure.SchemaReports
{
    /// <summary>
    /// An abstraction class for reports.
    /// </summary>
    public abstract class ReportBase
    {
        protected readonly string directoryName;
        protected readonly bool overwriteFiles = false;
        protected readonly Database database;

        /// <summary>
        /// Creates a new instance of the <see cref="ReportBase"/> class.
        /// </summary>
        /// <param name="database">The <see cref="Database"/>.</param>
        /// <param name="directoryName">The output directory name.</param>
        /// <param name="overwriteFiles">An indicator of whether to overwrite files when they exist.</param>
        public ReportBase(Database database, string directoryName, bool overwriteFiles = false)
        {
            this.database = database ?? throw new ArgumentNullException(nameof(database));
            this.directoryName = string.IsNullOrWhiteSpace(directoryName) ? throw new ArgumentNullException(nameof(directoryName)) : directoryName;
            this.overwriteFiles = overwriteFiles;
        }

        /// <summary>
        /// Generate the report.
        /// </summary>
        /// <returns>A task that represents the underlying operation.</returns>
        public abstract Task GenerateAsync();

        protected Stream CreateStream(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename)) { throw new ArgumentNullException(nameof(filename)); }
            CheckExistingFile(filename);

            return File.Create(filename);
        }

        protected void CheckExistingFile(string filename)
        {
            if (File.Exists(filename) && !overwriteFiles)
            {
                throw new Exception($"File '{filename}' already exists; use -o to overwrite.");
            }
        }

        protected string CleanupDbName(string databaseName)
        {
            return databaseName.Replace(" ", "_");
        }

        protected async Task WriteToStreamAsync(Stream stream, string message, bool flush = false)
        {
            if (stream != null && stream.CanWrite)
            {
                var buffer = Encoding.UTF8.GetBytes(message);
                await stream.WriteAsync(buffer, 0, buffer.Length);
                if (flush) { await stream.FlushAsync(); }
            }
        }

        protected async Task WriteLineToStreamAsync(Stream stream, string message, bool flush = false)
        {
            await WriteToStreamAsync(stream, $"{message}{Environment.NewLine}", flush);
        }

        protected async Task CloseStreamAsync(Stream stream)
        {
            if (stream != null && stream.CanWrite)
            {
                await stream.FlushAsync();
                stream.Close();
            }
        }
    }
}
