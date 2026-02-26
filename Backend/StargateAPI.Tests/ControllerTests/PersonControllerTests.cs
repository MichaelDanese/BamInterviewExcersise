using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Dtos;
using StargateAPI.Business.Queries;
using StargateAPI.Business.Services.Interfaces;
using StargateAPI.Controllers;
using System.Net;

namespace StargateAPI.Tests.ControllerTests
{
    public class PersonControllerTests
    {
        [Fact]
        public async Task GetPeople_ReturnsPeople_ReturnOkWhenPeopleExist()
        {
            var mockMediator = new Mock<IMediator>();
            var mockLogger = new Mock<IDatabaseLoggingService>();

            var expectedResult = new GetPeopleResult
            {
                Success = true,
                ResponseCode = (int)HttpStatusCode.OK,
                People = new List<PersonAstronaut>
                {
                    new PersonAstronaut
                    {
                        PersonId = 1,
                        Name = "John Doe",
                        CurrentRank = "Captain",
                        CurrentDutyTitle = "Commander",
                        CareerStartDate = new DateTime(2020, 1, 1)
                    },
                    new PersonAstronaut
                    {
                        PersonId = 2,
                        Name = "Jane Smith",
                        CurrentRank = null,
                        CurrentDutyTitle = null,
                        CareerStartDate = null
                    }
                }
            };

            mockMediator
                .Setup(m => m.Send(It.IsAny<GetPeople>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            var controller = new PersonController(mockMediator.Object, mockLogger.Object);

            var result = await controller.GetPeople();

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.OK, objectResult.StatusCode);
            var response = Assert.IsType<GetPeopleResult>(objectResult.Value);
            Assert.True(response.Success);
            Assert.NotNull(response.People);
            Assert.Equal(2, response.People.Count);
        }

        [Fact]
        public async Task GetPersonByName_ValidName_ReturnsOk()
        {
            var mockMediator = new Mock<IMediator>();
            var mockLogger = new Mock<IDatabaseLoggingService>();

            var expectedResult = new GetPersonByNameResult
            {
                Success = true,
                ResponseCode = (int)HttpStatusCode.OK,
                Person = new PersonAstronaut
                {
                    PersonId = 1,
                    Name = "John Doe",
                    CurrentRank = "Captain",
                    CurrentDutyTitle = "Commander",
                    CareerStartDate = new DateTime(2020, 1, 1),
                    CareerEndDate = null
                }
            };

            mockMediator
                .Setup(m => m.Send(It.IsAny<GetPersonByName>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            var controller = new PersonController(mockMediator.Object, mockLogger.Object);

            var result = await controller.GetPersonByName("John Doe");

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.OK, objectResult.StatusCode);
            var response = Assert.IsType<GetPersonByNameResult>(objectResult.Value);
            Assert.True(response.Success);
            Assert.NotNull(response.Person);
            Assert.Equal("John Doe", response.Person.Name);
        }

        [Fact]
        public async Task CreatePerson_ValidName_ReturnsOk()
        {
            var mockMediator = new Mock<IMediator>();
            var mockLogger = new Mock<IDatabaseLoggingService>();

            var expectedResult = new CreatePersonResult
            {
                Success = true,
                ResponseCode = (int)HttpStatusCode.OK,
                Id = 1,
                Message = "Person with the name of John Doe created successfully"
            };

            mockMediator
                .Setup(m => m.Send(It.IsAny<CreatePerson>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            var controller = new PersonController(mockMediator.Object, mockLogger.Object);

            var result = await controller.CreatePerson("John Doe");

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.OK, objectResult.StatusCode);
            var response = Assert.IsType<CreatePersonResult>(objectResult.Value);
            Assert.True(response.Success);
            Assert.NotEqual(0, response.Id);
            Assert.Contains("John Doe", response.Message);
        }
    }
}
