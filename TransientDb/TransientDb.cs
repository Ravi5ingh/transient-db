using System.Data.SqlClient;
using Dapper;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace TransientDb;

public static class TransientDb
{
    public static TransientDbConnection Create(params FileInfo[] sqlScripts)
    {
        // create db name
        var dbName = $"{Core.TransientDbNamePrefix}_{Guid.NewGuid().ToString().Substring(0, 8)}";

        // create db
        using (var tempConnection = new SqlConnection(Core.DefaultLocalDbConnectionString))
        {
            tempConnection.Execute($"create database {dbName}");
        }
        
        // create connection to return
        var transientDbConnection = new TransientDbConnection($"{Core.DefaultLocalDbConnectionString};Database={dbName};Pooling=false");
        
        // run scripts
        using (var sqlConnection = new Microsoft.Data.SqlClient.SqlConnection(transientDbConnection.Connection.ConnectionString))
        {
            var svrConnection = new ServerConnection(sqlConnection);
            var server = new Server(svrConnection);

            foreach (var script in sqlScripts)
            {
                try
                {
                    server.ConnectionContext.ExecuteNonQuery(File.ReadAllText(script.FullName));
                }
                catch (Exception e)
                {
                    var sqlExecutionError = e.InnerException != null
                        ? $"SQL Execution Error: {e.InnerException.Message}"
                        : string.Empty;

                    throw new Exception(
                        $"There was a problem running the SQL script: ({script.FullName}).\n\nProblem: {e.Message}\n\n{sqlExecutionError}\n\n",
                        e);
                }
            }
            
            sqlConnection.Close();
            svrConnection.Disconnect();
        }

        return transientDbConnection;
    }
}