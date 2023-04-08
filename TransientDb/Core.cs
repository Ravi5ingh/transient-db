using System.Data.SqlClient;
using Dapper;

namespace TransientDb;

internal static class Core
{
    internal static string DefaultLocalDbConnectionString = "Server=(LocalDb)\\MSSQLLocalDB";

    internal static string TransientDbNamePrefix = "TransientDb";
    
    internal static List<string> GetAllTransientDbs()
    {
        using (var tempConnection = new SqlConnection(DefaultLocalDbConnectionString))
        {
            return tempConnection
                .Query<string>("select name from master.dbo.sysdatabases where name like 'TransientDb%'")
                .ToList();
        }
    }

    internal static string GetDatabaseNameFromConnectionString(string connectionString)
    {
        var segments = connectionString.Split(";");
        return segments.First(seg => seg.StartsWith("Database")).Substring(9);
    }
    
    internal static string ConvertToMachineLevelConnectionString(string connectionString)
    {
        return string.Join(";", connectionString.Split(";").Where(seg => !seg.Contains("Database")));
    }

    internal static string DropDatabaseCommand(string dbName)
    {
        return $"drop database {dbName}";
    }
}