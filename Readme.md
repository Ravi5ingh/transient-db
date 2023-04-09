# TransientDb
TransientDb is a lightweight, dynamic, runtime, code-only database which comes in handy whenever you need quick setup and teardown of an ephemeral database with a controlled starting state specified by SQL scripts. It is based on MS SQL Local DB.

## Installing
- Ensure you have MS SQL Local DB. This is an optional install within SQL Express which you can install from: [SQL Server Express LocalDB - SQL Server | Microsoft Learn](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb?view=sql-server-ver16)
- Install the TransientDb nuget package from: [NOT YET PUBLISHED]

## Usage
TransientDb can be used to create and destroy local databases on the fly via SQL scripts. This is a very powerful concept when you want to create fully integrate tested code (all the way down the DB level) without having to share databases (which is un-controlled and un-scientific)

The following code shows an example:

```cs

	using(var transientDbConnection = TransientDb.Create(new FileInfo("CreateCompanyDatabase.sql")))
	{	
		// Transient database starts existing here
		
		// Dapper integration is seamless
		var dapperPerson = transientDbConnection.Connection.Query<Person>("select * from dbo.Persons where Id = 3).FirstOrDefault()
		
		// Create and point your EF DB context to your transient db
		var myEfDbContext = new CompanyContext(transientDbConnection.Connection.ConnectionString);
		var efPerson = myEfDbContext.Persons.Where(p => p.Id == 3).FirstOrDefault();
		
	}
	
	// Transient database ceases to exist here (cleaned up along with the connection)

```