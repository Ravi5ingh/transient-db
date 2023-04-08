using System.Data.SqlClient;
using Dapper;

namespace TransientDb;

public class TransientDbConnection : IDisposable
{
    public SqlConnection Connection { get; }

    public TransientDbConnection(string connectionString)
    {
        Connection = new SqlConnection(connectionString);
    }

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