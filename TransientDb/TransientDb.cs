using System;
using System.Data.SqlClient;
using System.IO;
using Dapper;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace TransientDatabase;

/// <summary>
/// TransientDb is a lightweight, dynamic, runtime, code-only database which comes in handy whenever you need quick setup and teardown of an ephemeral database 
/// with a controlled starting state specified by SQL scripts. It is based on MS SQL Local DB
/// </summary>
public static class TransientDb
{
    private static string DefaultTransientDbMdfFileLocation => "C:\\TransientDbs";
    
    /// <summary>
    /// Create a transient database, apply the SQL in the files provided, and return a disposable connection to the DB.
    /// Underlying DB is a MS SQL Local DB
    /// </summary>
    /// <param name="sqlScriptFilePaths">SQL script file paths</param>
    /// <returns>The connection to the transient db (whose disposal will tear down the transient db along with it)</returns>
    /// <exception cref="Exception">Throws exceptions depending on the contents of the scripts</exception>
    public static TransientDbConnection Create(params string[] sqlScriptFilePaths)
    {
        // create db name
        var dbName = $"{Core.TransientDbNamePrefix}_{Guid.NewGuid().ToString().Substring(0, 8)}";

        // create db
        CreateDatabase(Core.DefaultLocalDbConnectionString, dbName);
        
        // create connection to return
        var transientDbConnection = new TransientDbConnection($"{Core.DefaultLocalDbConnectionString};Database={dbName};Pooling=false");
        
        // run scripts
        using (var sqlConnection = new Microsoft.Data.SqlClient.SqlConnection(transientDbConnection.Connection.ConnectionString))
        {
            var svrConnection = new ServerConnection(sqlConnection);
            var server = new Server(svrConnection);

            foreach (var scriptFilePath in sqlScriptFilePaths)
            {
                try
                {
                    server.ConnectionContext.ExecuteNonQuery(File.ReadAllText(scriptFilePath));
                }
                catch (Exception e)
                {
                    var sqlExecutionError = e.InnerException != null
                        ? $"SQL Execution Error: {e.InnerException.Message}"
                        : string.Empty;

                    throw new Exception(
                        $"There was a problem running the SQL script: ({scriptFilePath}).\n\nProblem: {e.Message}\n\n{sqlExecutionError}\n\n",
                        e);
                }
            }
            
            sqlConnection.Close();
            svrConnection.Disconnect();
        }

        return transientDbConnection;
    }

    private static void CreateDatabase(string instanceConnectionString, string dbName)
    {
        // Ensure we have a place to put the MDF files
        if (!Directory.Exists(DefaultTransientDbMdfFileLocation))
        {
            Directory.CreateDirectory(DefaultTransientDbMdfFileLocation);
        }
        
        // Create DB
        using (var tempConnection = new SqlConnection(instanceConnectionString))
        {
            tempConnection.Execute($"create database {dbName} on (name='test', filename='{DefaultTransientDbMdfFileLocation}\\{dbName}.mdf')");
        }
    }
}