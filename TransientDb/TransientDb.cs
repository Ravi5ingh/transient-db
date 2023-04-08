using System.Data.SqlClient;
using Dapper;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace TransientDb;

/// <summary>
/// TransientDb is a lightweight, dynamic, runtime, code-only database which comes in handy whenever you need quick setup and teardown of an ephemeral database 
/// with a controlled starting state specified by SQL scripts. It is based on MS SQL Local DB
/// </summary>
public static class TransientDb
{
    /// <summary>
    /// Create a transient database, apply the SQL in the files provided, and return a disposable connection to the DB.
    /// Underlying DB is a MS SQL Local DB
    /// </summary>
    /// <param name="sqlScripts">SQL scripts</param>
    /// <returns>The connection to the transient db (whose disposal will tear down the transient db along with it)</returns>
    /// <exception cref="Exception">Throws exceptions depending on the contents of the scripts</exception>
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