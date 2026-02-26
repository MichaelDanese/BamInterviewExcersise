using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using StargateAPI.Business.Data;
using StargateAPI.Business.Queries;
using System.Formats.Asn1;

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

        public static async Task SeedTestPeopleAsync(StarbaseContext context)
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

        public static Mock<IMediator> SetupMediator(StarbaseContext context)
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
