using StargateAPI.Business.Data;
using StargateAPI.Business.Queries;
using StargateAPI.Tests.TestHelpers;

namespace StargateAPI.Tests.QueryTests
{
    public class GetPeopleTests
    {

        [Fact]
        public async Task GetPeople_ReturnsPeople_WhenPeopleExist()
        {
            var context = TestDbFactory.CreateInMemoryDbContext();

            // seed data
            await SeedTestPeopleAsync(context);

            var cancellationToken = CancellationToken.None;
            var handler = new GetPeopleHandler(context);
            var request = new GetPeople();
            var result = await handler.Handle(request, cancellationToken);

            Assert.NotNull(result);
            Assert.NotEmpty(result.People);
        }

        [Fact]
        public async Task GetPeople_ReturnsEmpty_WhenPeopleDoNotExist()
        {
            var context = TestDbFactory.CreateInMemoryDbContext();

            // skip seeding data
            var cancellationToken = CancellationToken.None;
            var handler = new GetPeopleHandler(context);
            var request = new GetPeople();
            var result = await handler.Handle(request, cancellationToken);

            Assert.NotNull(result);
            Assert.Empty(result.People);
        }

        private async Task SeedTestPeopleAsync(StarbaseContext context)
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
