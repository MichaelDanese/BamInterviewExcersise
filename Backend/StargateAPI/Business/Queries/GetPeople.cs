using Dapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Business.Dtos;
using StargateAPI.Controllers;

namespace StargateAPI.Business.Queries
{
    public class GetPeople : IRequest<GetPeopleResult>
    {

    }

    public class GetPeopleHandler : IRequestHandler<GetPeople, GetPeopleResult>
    {
        public readonly StarbaseContext _starbaseContext;
        public GetPeopleHandler(StarbaseContext context)
        {
            _starbaseContext = context;
        }

        /// <summary>
        /// Retrieves all people in the system with their current astronaut details, if assigned.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        public async Task<GetPeopleResult> Handle(GetPeople request, CancellationToken cancellationToken)
        {
            var result = new GetPeopleResult();

            var personAstronauts = await _starbaseContext.People
                    .AsNoTracking()
                    .Select(p => new PersonAstronaut
                    {
                        PersonId = p.Id,
                        Name = p.Name,
                        CurrentRank = p.AstronautDetail != null ? p.AstronautDetail.CurrentRank : null,
                        CurrentDutyTitle = p.AstronautDetail != null ? p.AstronautDetail.CurrentDutyTitle : null,
                        CareerStartDate = p.AstronautDetail != null ? p.AstronautDetail.CareerStartDate : null,
                        CareerEndDate = p.AstronautDetail != null ? p.AstronautDetail.CareerEndDate : null
                    })
                    .ToListAsync(cancellationToken);

            result.People = personAstronauts;
            return result;
        }
    }

    public class GetPeopleResult : BaseResponse
    {
        public List<PersonAstronaut> People { get; set; } = new List<PersonAstronaut> { };

    }
}
