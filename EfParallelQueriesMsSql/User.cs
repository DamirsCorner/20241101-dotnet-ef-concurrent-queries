namespace EfParallelQueriesMsSql;

internal class User(string username, string firstName, string lastName)
{
    public int Id { get; set; }
    public string Username { get; set; } = username;
    public string FirstName { get; set; } = firstName;
    public string LastName { get; set; } = lastName;
}
