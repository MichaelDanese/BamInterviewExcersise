using Microsoft.AspNetCore.Http;
using Moq;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Services.Interfaces;
using StargateAPI.Tests.TestHelpers;

namespace StargateAPI.Tests.CommandTests
{
    public class CreatePersonTests
    {
        [Fact]
        public async Task CreatePerson_ValidName_ShouldCreatePersonSuccessfully()
        {
            var context = TestDbFactory.CreateInMemoryDbContext();
            var preprocessor = new CreatePersonPreProcessor(context);
            var name = "John Doe";
            var request = new CreatePerson
            {
                Name = name
            };

            await preprocessor.Process(request, CancellationToken.None);

            var mockLogger = new Mock<IDatabaseLoggingService>();
            var handler = new CreatePersonHandler(context, mockLogger.Object);

            var result = await handler.Handle(request, CancellationToken.None);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotEqual(0, result.Id);
            Assert.Contains(name, result.Message);
        }

        [Fact]
        public async Task CreatePerson_Duplicate_ShouldThrowException()
        {
            var context = TestDbFactory.CreateInMemoryDbContext();
            var preprocessor = new CreatePersonPreProcessor(context);
            var name = "John Doe";
            var request = new CreatePerson
            {
                Name = name
            };

            await preprocessor.Process(request, CancellationToken.None);

            var mockLogger = new Mock<IDatabaseLoggingService>();
            var handler = new CreatePersonHandler(context, mockLogger.Object);

            var result = await handler.Handle(request, CancellationToken.None);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotEqual(0, result.Id);
            Assert.Contains(name, result.Message);

            await Assert.ThrowsAsync<BadHttpRequestException>(() =>
               preprocessor.Process(request, CancellationToken.None));
        }

        [Fact]
        public async Task CreatePerson_EmptyName_ShouldThrowException()
        {
            var context = TestDbFactory.CreateInMemoryDbContext();
            var preprocessor = new CreatePersonPreProcessor(context);
            var name = "";
            var request = new CreatePerson
            {
                Name = name
            };

            await Assert.ThrowsAsync<BadHttpRequestException>(() =>
               preprocessor.Process(request, CancellationToken.None));
        }

        [Fact]
        public async Task CreatePerson_NullRequest_ShouldThrowException()
        {
            var context = TestDbFactory.CreateInMemoryDbContext();
            var preprocessor = new CreatePersonPreProcessor(context);

            await Assert.ThrowsAsync<BadHttpRequestException>(() =>
               preprocessor.Process(null, CancellationToken.None));
        }
    }
}
