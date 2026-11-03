using Microsoft.Data.Sqlite;

namespace Lighthouse.Api.Data;

public static class DatabasePaths
{
    public static string Resolve(string connectionString, string contentRoot)
    {
        SqliteConnectionStringBuilder builder = new(connectionString);
        if (!Path.IsPathRooted(builder.DataSource))
        {
            builder.DataSource = Path.GetFullPath(Path.Combine(contentRoot, builder.DataSource));
        }
        return builder.ToString();
    }
}
