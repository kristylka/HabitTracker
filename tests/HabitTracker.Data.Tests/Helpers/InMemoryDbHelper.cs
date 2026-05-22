namespace HabitTracker.Data.Tests.Helpers;

using HabitTracker.Data;
using Microsoft.EntityFrameworkCore;

public static class InMemoryDbHelper
{
    public static AppDbContext CreateContext(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName ?? Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
    public static IDbContextFactory<AppDbContext> CreateFactory(string? dbName = null)
    {
        var name = dbName ?? Guid.NewGuid().ToString();
        return new TestDbContextFactory(name);
    }

    private class TestDbContextFactory : IDbContextFactory<AppDbContext>
    {
        private readonly string _dbName;

        public TestDbContextFactory(string dbName)
        {
            _dbName = dbName;
        }

        public AppDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: _dbName)
                .Options;
            return new AppDbContext(options);
        }
    }
}