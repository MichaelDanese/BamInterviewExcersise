using MediatR;
using Microsoft.AspNetCore.Mvc;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Queries;
using StargateAPI.Business.Services.Interfaces;
using System.Net;

namespace StargateAPI.Controllers
{
   
    [ApiController]
    [Route("[controller]")]
    public class PersonController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IDatabaseLoggingService _logger;
        public PersonController(IMediator mediator, IDatabaseLoggingService logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet("")]
        public async Task<IActionResult> GetPeople()
        {
            try
            {
                var result = await _mediator.Send(new GetPeople()
                {

                });

                return this.GetResponse(result);
            }
            catch (BadHttpRequestException ex)
            {
                await _logger.LogErrorAsync("Error in GetPeople", ex.Message, ex);

                return this.GetResponse(new BaseResponse()
                {
                    Message = ex.Message,
                    Success = false,
                    ResponseCode = (int)HttpStatusCode.BadRequest
                });
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync("Error in GetPeople", ex.Message, ex);

                return this.GetResponse(new BaseResponse()
                {
                    Message = "An internal server error occurred. Please try again later.",
                    Success = false,
                    ResponseCode = (int)HttpStatusCode.InternalServerError
                });
            }
        }

        [HttpGet("{name}")]
        public async Task<IActionResult> GetPersonByName(string name)
        {
            try
            {
                var result = await _mediator.Send(new GetPersonByName()
                {
                    Name = name
                });

                return this.GetResponse(result);
            }
            catch (BadHttpRequestException ex)
            {
                await _logger.LogErrorAsync("Error in GetPersonByName", ex.Message, ex);

                return this.GetResponse(new BaseResponse()
                {
                    Message = ex.Message,
                    Success = false,
                    ResponseCode = (int)HttpStatusCode.BadRequest
                });
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync("Error in GetPersonByName", ex.Message, ex);

                return this.GetResponse(new BaseResponse()
                {
                    Message = "An internal server error occurred. Please try again later.",
                    Success = false,
                    ResponseCode = (int)HttpStatusCode.InternalServerError
                });
            }
        }

        [HttpPost("")]
        public async Task<IActionResult> CreatePerson([FromBody] string name)
        {
            try
            {
                var result = await _mediator.Send(new CreatePerson()
                {
                    Name = name
                });

                return this.GetResponse(result);
            }
            catch (BadHttpRequestException ex)
            {
                await _logger.LogErrorAsync("Error in CreatePerson", ex.Message, ex);

                return this.GetResponse(new BaseResponse()
                {
                    Message = ex.Message,
                    Success = false,
                    ResponseCode = (int)HttpStatusCode.BadRequest
                });
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync("Error in CreatePerson", ex.Message, ex);

                return this.GetResponse(new BaseResponse()
                {
                    Message = "An internal server error occurred. Please try again later.",
                    Success = false,
                    ResponseCode = (int)HttpStatusCode.InternalServerError
                });
            }

        }
    }
}