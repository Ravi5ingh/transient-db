using System.Data.SqlClient;
using Dapper;

namespace TransientDatabase;

/// <summary>
/// A connection to an instance of TransientDb. This is a disposable class which will take the DB down with it when disposed of
/// </summary>
public class TransientDbConnection : IDisposable
{
    /// <summary>
    /// The underlying SQL connection
    /// </summary>
    public SqlConnection Connection { get; }

    /// <summary>
    /// .ctor
    /// </summary>
    /// <param name="connectionString">The connection string to the Transient DB</param>
    internal TransientDbConnection(string connectionString)
    {
        Connection = new SqlConnection(connectionString);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        var closedConnectionString = Connection.ConnectionString;
        Connection.Dispose();

        var thisDbName = string.Empty;

        try
        {
            // Close current connection and elevate connection to machine level to close database we were pointing at
            thisDbName = Core.GetDatabaseNameFromConnectionString(closedConnectionString);
            var machineLevelConnectionString = Core.ConvertToMachineLevelConnectionString(closedConnectionString);

            using (var sqlConnection = new SqlConnection(machineLevelConnectionString))
            {
                sqlConnection.Execute(Core.DropDatabaseCommand(thisDbName));
            }
        }
        catch (SqlException sqlException)
        {
            // If the DB hasn't been cleaned up by something else, only then throw
            if (Core.GetAllTransientDbs().Contains(thisDbName))
            {
                throw sqlException;
            }
        }
    }
}