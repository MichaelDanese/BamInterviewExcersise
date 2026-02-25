using MediatR;
using Microsoft.AspNetCore.Http;
using Moq;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using StargateAPI.Business.Queries;
using StargateAPI.Business.Services.Interfaces;
using StargateAPI.Tests.TestHelpers;
using static System.Net.WebRequestMethods;

namespace StargateAPI.Tests.CommandTests
{
    public class CreateAstronautDutyTests
    {
        [Fact]
        public async Task CreateAstronautDuty_NoCurrentDuty_ShouldCreateDutySuccessfully()
        {
            var context = TestDbFactory.CreateInMemoryDbContext();
            await SeedTestPeopleAsync(context);

            var preprocessor = new CreateAstronautDutyPreProcessor(context);
            var request = new CreateAstronautDuty
            {
                Name = "Steve",
                Rank = "Lieutenant",
                DutyTitle = "Pilot",
                DutyStartDate = DateTime.UtcNow
            };

            await preprocessor.Process(request, CancellationToken.None);
            var mockMediator = SetupMediator(context);
            var mockLogger = new Mock<IDatabaseLoggingService>();

            var handler = new CreateAstronautDutyHandler(context, mockMediator.Object, mockLogger.Object);

            var result = await handler.Handle(request, CancellationToken.None);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Id);
            Assert.NotEqual(0, result.Id);
        }

        [Fact]
        public async Task CreateAstronautDuty_NoCurrentDuty_ShouldCreateDutySuccessfullyAndThenThrowExceptionOnSameDayRetirement()
        {
            var context = TestDbFactory.CreateInMemoryDbContext();
            await SeedTestPeopleAsync(context);

            var preprocessor = new CreateAstronautDutyPreProcessor(context);
            var request = new CreateAstronautDuty
            {
                Name = "Steve",
                Rank = "Lieutenant",
                DutyTitle = "Pilot",
                DutyStartDate = DateTime.UtcNow
            };

            await preprocessor.Process(request, CancellationToken.None);
            var mockMediator = SetupMediator(context);
            var mockLogger = new Mock<IDatabaseLoggingService>();

            var handler = new CreateAstronautDutyHandler(context, mockMediator.Object, mockLogger.Object);

            var result = await handler.Handle(request, CancellationToken.None);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Id);
            Assert.NotEqual(0, result.Id);

            var requestTwo = request = new CreateAstronautDuty
            {
                Name = "Steve",
                Rank = "Lieutenant",
                DutyTitle = "RETIRED",
                DutyStartDate = DateTime.UtcNow
            };

            await Assert.ThrowsAsync<BadHttpRequestException>(() =>
               preprocessor.Process(requestTwo, CancellationToken.None));
        }

        [Fact]
        public async Task CreateAstronautDuty_NoCurrentDutyRetirement_ThrowException()
        {
            var context = TestDbFactory.CreateInMemoryDbContext();
            await SeedTestPeopleAsync(context);

            var preprocessor = new CreateAstronautDutyPreProcessor(context);
            var request = new CreateAstronautDuty
            {
                Name = "Steve",
                Rank = "Lieutenant",
                DutyTitle = "Retired",
                DutyStartDate = DateTime.UtcNow
            };

            await Assert.ThrowsAsync<BadHttpRequestException>(() =>
               preprocessor.Process(request, CancellationToken.None));

        }

        [Fact]
        public async Task CreateAstronautDuty_ActiveDuty_ShouldCreateDutySuccessfully()
        {
            var context = TestDbFactory.CreateInMemoryDbContext();
            await SeedTestPeopleAsync(context);

            var preprocessor = new CreateAstronautDutyPreProcessor(context);
            var name = "John Doe";
            var request = new CreateAstronautDuty
            {
                Name = name,
                Rank = "Master Commander",
                DutyTitle = "Master Pilot",
                DutyStartDate = DateTime.UtcNow
            };

            await preprocessor.Process(request, CancellationToken.None);
            var mockMediator = SetupMediator(context);
            var mockLogger = new Mock<IDatabaseLoggingService>();

            var handler = new CreateAstronautDutyHandler(context, mockMediator.Object, mockLogger.Object);

            var result = await handler.Handle(request, CancellationToken.None);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Id);
            Assert.NotEqual(0, result.Id);

            // Check the rule "A Person will only ever hold one current Astronaut Duty Title, Start Date, and Rank at a time."
            var dutiesRequest = new GetAstronautDutiesByName { Name = name };
            var dutiesHandler = new GetAstronautDutiesByNameHandler(context, mockMediator.Object);
            var dutiesResult = await dutiesHandler.Handle(dutiesRequest, CancellationToken.None);

            Assert.True(dutiesResult.Success);
            Assert.NotNull(dutiesResult.AstronautDuties);
            var activeDuties = dutiesResult.AstronautDuties.Where(d => d.DutyEndDate == null).ToList();
            Assert.Single(activeDuties);
        }

        [Fact]
        public async Task CreateAstronautDuty_Retirement_ShouldCreateDutySuccessfully()
        {
            var context = TestDbFactory.CreateInMemoryDbContext();
            await SeedTestPeopleAsync(context);

            var preprocessor = new CreateAstronautDutyPreProcessor(context);
            var name = "John Doe";
            var request = new CreateAstronautDuty
            {
                Name = name,
                Rank = "Master Commander",
                DutyTitle = "RETIRED",
                DutyStartDate = DateTime.UtcNow
            };

            await preprocessor.Process(request, CancellationToken.None);
            var mockMediator = SetupMediator(context);
            var mockLogger = new Mock<IDatabaseLoggingService>();

            var handler = new CreateAstronautDutyHandler(context, mockMediator.Object, mockLogger.Object);

            var result = await handler.Handle(request, CancellationToken.None);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Id);
            Assert.NotEqual(0, result.Id);

            var personHandler = new GetPersonByNameHandler(context);

            // Check the rule "A Person's Career End Date is one day before the Retired Duty Start Date."
            var personResult = await personHandler.Handle(new GetPersonByName { Name = name }, CancellationToken.None);
            Assert.NotNull(personResult);
            Assert.NotNull(personResult.Person);
            Assert.Equal("RETIRED", personResult.Person.CurrentDutyTitle);
            Assert.Equal(DateTime.UtcNow.AddDays(-1).Date, personResult.Person.CareerEndDate);
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
                },
                AstronautDuties = new List<AstronautDuty>
                {
                    new AstronautDuty
                    {
                        Rank = "Lieutenant",
                        DutyTitle = "Pilot",
                        DutyStartDate = new DateTime(2020, 1, 1),
                        DutyEndDate = new DateTime(2021, 1, 1)
                    },
                    new AstronautDuty
                    {
                        Rank = "Captain",
                        DutyTitle = "Commander",
                        DutyStartDate = new DateTime(2021, 1, 2),
                        DutyEndDate = null
                    }
                }
            });

            await context.AddAsync(new Person
            {
                Name = "Steve"
            });

            await context.SaveChangesAsync();
        }

        private Mock<IMediator> SetupMediator(StarbaseContext context)
        {
            var mockMediator = new Mock<IMediator>();
            var personHandler = new GetPersonByNameHandler(context);

            mockMediator.Setup(m => m.Send(It.IsAny<GetPersonByName>(), It.IsAny<CancellationToken>()))
                .Returns<GetPersonByName, CancellationToken>(async (request, cancellationToken) =>
                {
                    var result = await personHandler.Handle(request, cancellationToken);
                    return result;
                });

            return mockMediator;
        }
    }
}
