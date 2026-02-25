using Moq;
using StargateAPI.Business.Data;
using StargateAPI.Business.Queries;
using StargateAPI.Tests.TestHelpers;

namespace StargateAPI.Tests.QueryTests
{
    public class GetPersonByNameTests
    {
        [Fact]
        public async Task GetPersonByName_ReturnsPerson_WhenPersonExists()
        {
            var context = TestDbFactory.CreateInMemoryDbContext();

            // seed data
            await SeedTestPersonAsync(context);

            var request = new GetPersonByName { Name = "John Doe" };
            var cancellationToken = CancellationToken.None;

            var handler = new GetPersonByNameHandler(context);

            var result = await handler.Handle(request, cancellationToken);

            Assert.NotNull(result);
            Assert.NotNull(result.Person);
            Assert.Equal("John Doe", result.Person.Name);
        }

        [Fact]
        public async Task GetPersonByName_ReturnsNullPerson_WhenPersonDoesNotExist()
        {
            var context = TestDbFactory.CreateInMemoryDbContext();

            // seed data
            await SeedTestPersonAsync(context);

            var request = new GetPersonByName { Name = "Jane Doe" };
            var cancellationToken = CancellationToken.None;

            var handler = new GetPersonByNameHandler(context);

            var result = await handler.Handle(request, cancellationToken);

            Assert.NotNull(result);
            Assert.Null(result.Person);
        }

        private async Task SeedTestPersonAsync(StarbaseContext context)
        {
            await context.AddAsync(new Person
            {
                Name = "John Doe",
                AstronautDetail = new AstronautDetail
                {
                    CurrentRank = "Captain",
                    CurrentDutyTitle = "Commander",
                    CareerStartDate = new DateTime(2020, 1, 1)
                }
            });

            await context.SaveChangesAsync();
        }
    }
}