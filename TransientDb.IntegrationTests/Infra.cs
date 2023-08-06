using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace TransientDatabase.IntegrationTests;

public static class Infra
{
    public static void DeleteAllTransientDbs(SqlConnection sqlConnection)
    {
        var transientDbs = GetAllTransientDbs(sqlConnection);

        foreach (var db in transientDbs)
        {
            sqlConnection.Execute($"drop database {db}");
        }
    }

    public static List<string> GetAllTransientDbs(SqlConnection sqlConnection)
    {
        return sqlConnection.Query<string>(
                "select name from master.dbo.sysdatabases where name like 'TransientDb%'").ToList();
    }

    public static List<string> GetAllTablesInDb(SqlConnection sqlConnection)
    {
        return sqlConnection.Query<string>("select table_name from information_schema.tables;").ToList();
    }
}