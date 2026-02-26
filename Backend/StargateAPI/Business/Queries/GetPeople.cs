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

            var today = DateTime.UtcNow.Date;

            var personAstronauts = await _starbaseContext.People
                .AsNoTracking()
                .GroupJoin(
                    _starbaseContext.AstronautDuties
                        .Where(ad =>
                            ad.DutyStartDate.Date <= today &&
                            (ad.DutyEndDate == null || ad.DutyEndDate.Value.Date >= today)),
                    person => person.Id,
                    duty => duty.PersonId,
                    (person, duties) => new
                    {
                        Person = person,
                        CurrentDuty = duties.OrderByDescending(d => d.DutyStartDate).FirstOrDefault()
                    })
                .Select(x => new PersonAstronaut
                {
                    PersonId = x.Person.Id,
                    Name = x.Person.Name,
                    CurrentRank = x.CurrentDuty != null ? x.CurrentDuty.Rank : null,
                    CurrentDutyTitle = x.CurrentDuty != null ? x.CurrentDuty.DutyTitle : null,
                    CareerStartDate = x.Person.AstronautDetail != null ? x.Person.AstronautDetail.CareerStartDate : null,
                    CareerEndDate = x.Person.AstronautDetail != null ? x.Person.AstronautDetail.CareerEndDate : null
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
