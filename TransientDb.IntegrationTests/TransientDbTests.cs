using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Shouldly;
using Xunit;

namespace TransientDatabase.IntegrationTests
{
    public class TransientDbTests : IDisposable
    {
        private readonly SqlConnection _localDbConnection;
        
        /// <summary>
        /// Setup
        /// </summary>
        public TransientDbTests()
        {
            _localDbConnection = new SqlConnection("Server=(LocalDb)\\MSSQLLocalDB");
            
            Infra.DeleteAllTransientDbs(_localDbConnection);
        }
        
        /// <summary>
        /// Teardown
        /// </summary>
        public void Dispose()
        {
            Infra.DeleteAllTransientDbs(_localDbConnection);
            
            _localDbConnection?.Dispose();
        }

        [Fact]
        public void Create_MultipleInvokes_CreatesDifferentDbEachTime()
        {
            var script = new FileInfo("TransientDbTests.sql");

            using(var transientDbConnection1 = TransientDb.Create(script))
            using(var transientDbConnection2 = TransientDb.Create(script))
            using(var transientDbConnection3 = TransientDb.Create(script))
            {
                var transientDbs = Infra.GetAllTransientDbs(_localDbConnection);
                
                transientDbs.Count.ShouldBe(3);
                transientDbs.ShouldAllBe(db => db.StartsWith("TransientDb"));

                var tables1 = Infra.GetAllTablesInDb(transientDbConnection1.Connection);
                var tables2 = Infra.GetAllTablesInDb(transientDbConnection2.Connection);
                var tables3 = Infra.GetAllTablesInDb(transientDbConnection3.Connection);

                tables1.ShouldHaveSingleItem();
                tables1.First().ShouldBe("Persons");
                
                tables2.ShouldHaveSingleItem();
                tables2.First().ShouldBe("Persons");
                
                tables3.ShouldHaveSingleItem();
                tables3.First().ShouldBe("Persons");
            }
        }

        [Fact]
        public void Create_SimpleCase_CleansUpDbOnDispose()
        {
            using (var transientDbConnection = TransientDb.Create(new FileInfo("TransientDbTests.sql")))
            {
                var tables = Infra.GetAllTablesInDb(transientDbConnection.Connection);

                tables.ShouldHaveSingleItem();
                tables.First().ShouldBe("Persons");
            }
            
            using (var sqlConnection = new SqlConnection("Server=(LocalDb)\\MSSQLLocalDB"))
            {
                var transientDbNames = Infra.GetAllTransientDbs(sqlConnection);
                    
                transientDbNames.ShouldBeEmpty();
            }
        }
    }
}