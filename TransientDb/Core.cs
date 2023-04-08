using System.Data.SqlClient;
using Dapper;
namespace TransientDb;

/// <summary>
/// Core functionality used by the TransientDb library
/// </summary>
internal static class Core
{
    /// <summary>
    /// Default connection string prefix (Hard-bound to SQL Local DB)
    /// </summary>
    internal static string DefaultLocalDbConnectionString = "Server=(LocalDb)\\MSSQLLocalDB";

    /// <summary>
    /// TransientDb name prefix
    /// </summary>
    internal static string TransientDbNamePrefix = "TransientDb";
    
    /// <summary>
    /// Get all TransientDb instance present on the local machine
    /// </summary>
    /// <returns>The names of all TransientDb instances</returns>
    internal static List<string> GetAllTransientDbs()
    {
        using (var tempConnection = new SqlConnection(DefaultLocalDbConnectionString))
        {
            return tempConnection
                .Query<string>("select name from master.dbo.sysdatabases where name like 'TransientDb%'")
                .ToList();
        }
    }

    /// <summary>
    /// Get the DB name from any SQL connection string
    /// </summary>
    /// <param name="connectionString">SQL connection string</param>
    /// <returns>The database name</returns>
    internal static string GetDatabaseNameFromConnectionString(string connectionString)
    {
        var segments = connectionString.Split(";");
        return segments.First(seg => seg.StartsWith("Database")).Substring(9);
    }
    
    /// <summary>
    /// Convert any SQL connection string to a machine level connection string (Not bound to any DB)
    /// </summary>
    /// <param name="connectionString">Any SQL connection string</param>
    /// <returns>A machine level SQL connection string</returns>
    internal static string ConvertToMachineLevelConnectionString(string connectionString)
    {
        return string.Join(";", connectionString.Split(";").Where(seg => !seg.Contains("Database")));
    }

    /// <summary>
    /// Drop database command for a given DB name
    /// </summary>
    /// <param name="dbName">The DB name</param>
    /// <returns>The drop command</returns>
    internal static string DropDatabaseCommand(string dbName)
    {
        return $"drop database {dbName}";
    }
}