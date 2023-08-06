create table Persons(
    PersonId int,
    Firstname varchar(50),
    Lastname varchar(50),
    City varchar(255)
)

insert into dbo.Persons
(PersonId, FirstName, LastName, City)
values
(1, 'Ravi', 'Singh', 'London'),
(2, 'John', 'Smith', 'Someville')