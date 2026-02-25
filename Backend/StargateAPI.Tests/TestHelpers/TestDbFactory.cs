using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using StargateAPI.Business.Data;

namespace StargateAPI.Tests.TestHelpers
{
    public class TestDbFactory
    {
        public static StarbaseContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<StarbaseContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            var context = new StarbaseContext(options);
            context.Database.EnsureCreated();
            return context;
        }
    }
}
