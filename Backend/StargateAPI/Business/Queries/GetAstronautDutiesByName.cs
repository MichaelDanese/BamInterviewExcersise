using Dapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Business.Dtos;
using StargateAPI.Controllers;

namespace StargateAPI.Business.Queries
{
    public class GetAstronautDutiesByName : IRequest<GetAstronautDutiesByNameResult>
    {
        public string Name { get; set; } = string.Empty;
    }

    public class GetAstronautDutiesByNameHandler : IRequestHandler<GetAstronautDutiesByName, GetAstronautDutiesByNameResult>
    {
        private readonly StarbaseContext _context;
        private readonly IMediator _mediator;

        public GetAstronautDutiesByNameHandler(StarbaseContext context, IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
        }

        /// <summary>
        /// Retrieves a person's astronaut duty history by their name, including current assignment details and all duty records ordered by start date.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>
        /// A <see cref="GetAstronautDutiesByNameResult"/> containing:
        /// - Person: Current astronaut details
        /// - AstronautDuties: List of all duty assignments (empty if person has no duties)
        /// Returns an empty result if the provided name is null or whitespace.
        /// </returns>
        public async Task<GetAstronautDutiesByNameResult> Handle(GetAstronautDutiesByName request, CancellationToken cancellationToken)
        {
            var result = new GetAstronautDutiesByNameResult();
            var normalizedName = request?.Name?.Trim();

            if (string.IsNullOrEmpty(normalizedName))
            {
                return result;
            }

            var personResult = await _mediator.Send(new GetPersonByName { Name = request.Name }, cancellationToken);

            result.Person = personResult?.Person;

            if (personResult?.Person != null)
            {
                var person = personResult.Person;

                var duties = await _context.AstronautDuties
                    .AsNoTracking()
                    .Where(d => d.PersonId == person.PersonId)
                    .OrderByDescending(d => d.DutyStartDate)
                    .Select(d => new AstronautDutyDTO
                    {
                        Rank = d.Rank,
                        DutyTitle = d.DutyTitle,
                        DutyStartDate = d.DutyStartDate,
                        DutyEndDate = d.DutyEndDate
                    })
                    .ToListAsync(cancellationToken);

                result.AstronautDuties = duties;
            }

            return result;

        }
    }

    public class GetAstronautDutiesByNameResult : BaseResponse
    {
        public PersonAstronaut Person { get; set; }
        public List<AstronautDutyDTO> AstronautDuties { get; set; } = new List<AstronautDutyDTO>();
    }
}
