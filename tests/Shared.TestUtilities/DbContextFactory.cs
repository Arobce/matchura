using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Shared.TestUtilities;

public static class DbContextFactory
{
    public static (T Context, SqliteConnection Connection) Create<T>() where T : DbContext
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<T>()
            .UseSqlite(connection)
            .Options;

        var context = (T)Activator.CreateInstance(typeof(T), options)!;
        context.Database.EnsureCreated();
        return (context, connection);
    }
}
