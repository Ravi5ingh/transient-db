using Dapper;
using Shouldly;
using TransientDatabase;

namespace NugetClient;

public class Tests
{
    [Fact]
    public void should_be_able_to_create_and_use_transientdb()
    {
        using (var transientDbConnection = TransientDb.Create(new FileInfo("Tests.sql")))
        {
            var people = transientDbConnection.Connection.Query<Person>("select * from dbo.Persons").ToList();
            
            people.Count.ShouldBe(2);
            
            people[0].PersonId.ShouldBe(1);
            people[0].FirstName.ShouldBe("Ravi");
            people[0].LastName.ShouldBe("Singh");
            people[0].City.ShouldBe("London");
            
            people[1].PersonId.ShouldBe(2);
            people[1].FirstName.ShouldBe("John");
            people[1].LastName.ShouldBe("Smith");
            people[1].City.ShouldBe("Someville");
        }
    }
}