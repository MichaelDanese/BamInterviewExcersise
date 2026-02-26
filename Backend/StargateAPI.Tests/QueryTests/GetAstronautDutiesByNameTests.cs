using MediatR;
using Moq;
using StargateAPI.Business.Data;
using StargateAPI.Business.Queries;
using StargateAPI.Tests.TestHelpers;

namespace StargateAPI.Tests.QueryTests
{
    public class GetAstronautDutiesByNameTests
    {
        [Fact]
        public async Task GetAstronautDutiesByName_ReturnsPerson_WhenPersonExists()
        {
            var context = TestDbFactory.CreateInMemoryDbContext();

            // seed data
            await TestDbFactory.SeedTestPeopleAsync(context);

            var request = new GetAstronautDutiesByName { Name = "John Doe" };
            var cancellationToken = CancellationToken.None;

            var mockMediator = TestDbFactory.SetupMediator(context);
            var handler = new GetAstronautDutiesByNameHandler(context, mockMediator.Object);

            var result = await handler.Handle(request, cancellationToken);

            Assert.NotNull(result);
            Assert.NotNull(result.Person);
            Assert.NotNull(result.AstronautDuties);
            Assert.NotEmpty(result.AstronautDuties);
            Assert.Equal("John Doe", result.Person.Name);
        }

        [Fact]
        public async Task GetAstronautDutiesByName_ReturnsNullPerson_WhenPersonDoesNotExist()
        {
            var context = TestDbFactory.CreateInMemoryDbContext();

            // seed data
            await TestDbFactory.SeedTestPeopleAsync(context);

            var request = new GetAstronautDutiesByName { Name = "Jane Doe" };
            var cancellationToken = CancellationToken.None;

            var mockMediator = TestDbFactory.SetupMediator(context);

            var handler = new GetAstronautDutiesByNameHandler(context, mockMediator.Object);

            var result = await handler.Handle(request, cancellationToken);

            Assert.NotNull(result);
            Assert.Null(result.Person);
            Assert.NotNull(result.AstronautDuties);
            Assert.Empty(result.AstronautDuties);
        }

        [Fact]
        public async Task GetAstronautDutiesByName_ReturnsPersonAndEmptyDuties_WhenPersonExistsByNoDuties()
        {
            var context = TestDbFactory.CreateInMemoryDbContext();

            // seed data
            await TestDbFactory.SeedTestPeopleAsync(context);

            var request = new GetAstronautDutiesByName { Name = "Steve" };
            var cancellationToken = CancellationToken.None;

            var mockMediator = TestDbFactory.SetupMediator(context);

            var handler = new GetAstronautDutiesByNameHandler(context, mockMediator.Object);

            var result = await handler.Handle(request, cancellationToken);

            Assert.NotNull(result);
            Assert.NotNull(result.Person);
            Assert.NotNull(result.AstronautDuties);
            Assert.Empty(result.AstronautDuties);
            Assert.Equal("Steve", result.Person.Name);
        }
    }
}
