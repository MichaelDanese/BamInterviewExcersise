using MediatR;
using Microsoft.AspNetCore.Http;
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
    public class AstronautDutyControllerTests
    {
        [Fact]
        public async Task GetAstronautDutiesByName_ValidName_ReturnsOkWithDuties()
        {
            var mockMediator = new Mock<IMediator>();
            var mockLogger = new Mock<IDatabaseLoggingService>();

            var expectedResult = new GetAstronautDutiesByNameResult
            {
                Success = true,
                ResponseCode = (int)HttpStatusCode.OK,
                Person = new PersonAstronaut
                {
                    PersonId = 1,
                    Name = "John Doe",
                    CurrentRank = "Captain",
                    CurrentDutyTitle = "Commander",
                    CareerStartDate = new DateTime(2020, 1, 1)
                },
                AstronautDuties = new List<AstronautDutyDTO>
                {
                    new AstronautDutyDTO
                    {
                        Rank = "Lieutenant",
                        DutyTitle = "Pilot",
                        DutyStartDate = new DateTime(2020, 1, 1),
                        DutyEndDate = new DateTime(2021, 1, 1)
                    }
                }
            };

            mockMediator
                .Setup(m => m.Send(It.IsAny<GetAstronautDutiesByName>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            var controller = new AstronautDutyController(mockMediator.Object, mockLogger.Object);

            var result = await controller.GetAstronautDutiesByName("John                 Doe");

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.OK, objectResult.StatusCode);
            var response = Assert.IsType<GetAstronautDutiesByNameResult>(objectResult.Value);
            Assert.True(response.Success);
            Assert.NotNull(response.Person);
            Assert.NotEmpty(response.AstronautDuties);
        }

        [Fact]
        public async Task GetAstronautDutiesByName_ExceptionThrown_ReturnsInternalServerError()
        {
            var mockMediator = new Mock<IMediator>();
            var mockLogger = new Mock<IDatabaseLoggingService>();

            mockMediator
                .Setup(m => m.Send(It.IsAny<GetAstronautDutiesByName>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            var controller = new AstronautDutyController(mockMediator.Object, mockLogger.Object);

            var result = await controller.GetAstronautDutiesByName("John Doe");

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.InternalServerError, objectResult.StatusCode);
            var response = Assert.IsType<BaseResponse>(objectResult.Value);
            Assert.False(response.Success);
        }

        [Fact]
        public async Task CreateAstronautDuty_ValidRequest_ReturnsOkWithId()
        {
            var mockMediator = new Mock<IMediator>();
            var mockLogger = new Mock<IDatabaseLoggingService>();

            var expectedResult = new CreateAstronautDutyResult
            {
                Success = true,
                ResponseCode = (int)HttpStatusCode.OK,
                Id = 1,
                Message = "Astronaut duty created successfully"
            };

            mockMediator
                .Setup(m => m.Send(It.IsAny<CreateAstronautDuty>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            var controller = new AstronautDutyController(mockMediator.Object, mockLogger.Object);

            var request = new CreateAstronautDuty
            {
                Name = "John Doe",
                Rank = "Captain",
                DutyTitle = "Commander",
                DutyStartDate = DateTime.UtcNow
            };

            var result = await controller.CreateAstronautDuty(request);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.OK, objectResult.StatusCode);
            var response = Assert.IsType<CreateAstronautDutyResult>(objectResult.Value);
            Assert.True(response.Success);
            Assert.NotNull(response.Id);
            Assert.Equal(1, response.Id);
        }

        [Fact]
        public async Task CreateAstronautDuty_BadRequest_ReturnsBadRequest()
        {
            var mockMediator = new Mock<IMediator>();
            var mockLogger = new Mock<IDatabaseLoggingService>();

            mockMediator
                .Setup(m => m.Send(It.IsAny<CreateAstronautDuty>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new BadHttpRequestException("Person name cannot be empty"));

            var controller = new AstronautDutyController(mockMediator.Object, mockLogger.Object);

            var request = new CreateAstronautDuty
            {
                Name = "",
                Rank = "Captain",
                DutyTitle = "Commander",
                DutyStartDate = DateTime.UtcNow
            };

            var result = await controller.CreateAstronautDuty(request);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.BadRequest, objectResult.StatusCode);
            var response = Assert.IsType<BaseResponse>(objectResult.Value);
            Assert.False(response.Success);
        }
    }
}

