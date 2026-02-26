using MediatR;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Business.Dtos;
using StargateAPI.Business.Extensions;
using StargateAPI.Business.Services.Interfaces;
using StargateAPI.Controllers;

namespace StargateAPI.Business.Queries
{
    public class GetPersonByName : IRequest<GetPersonByNameResult>
    {
        public required string Name { get; set; } = string.Empty;
    }

    public class GetPersonByNameHandler : IRequestHandler<GetPersonByName, GetPersonByNameResult>
    {
        private readonly StarbaseContext _starbaseContext;
        public GetPersonByNameHandler(StarbaseContext context)
        {
            _starbaseContext = context;
        }

        /// <summary>
        /// Retrieves a person and their astronaut details by their name. 
        /// Trims and converts provided name to lowercase for comparison.
        /// </summary>
        /// <returns>Result containing the person's details and current astronaut assignment, if any. Will return null if there is no match.</returns>
        public async Task<GetPersonByNameResult> Handle(GetPersonByName request, CancellationToken cancellationToken)
        {
            var result = new GetPersonByNameResult();
            var normalizedName = request?.Name.NormalizeNameOrTitle().ToLower();

            if (String.IsNullOrEmpty(normalizedName))
            {
                return result;
            }

            var today = DateTime.UtcNow.Date;

            var person = await _starbaseContext.People
                .Include(p => p.AstronautDetail)
                .AsNoTracking()
                .Where(p => p.Name.ToLower() == normalizedName)
                .FirstOrDefaultAsync(cancellationToken);

            if (person == null)
            {
                return result;
            }

            // first, try to find a duty where today falls within the date range
            // if there are multiple active duties, we will take the most recent one based on DutyStartDate
            var currentDuty = await _starbaseContext.AstronautDuties
                .AsNoTracking()
                .Where(ad =>
                    ad.PersonId == person.Id &&
                    ad.DutyStartDate.Date <= today &&
                    (ad.DutyEndDate == null || ad.DutyEndDate.Value.Date >= today))
                .OrderByDescending(ad => ad.DutyStartDate)
                .FirstOrDefaultAsync(cancellationToken);

            var personAstronaut = new PersonAstronaut
            {
                PersonId = person.Id,
                Name = person.Name,
                CurrentRank = currentDuty?.Rank,
                CurrentDutyTitle = currentDuty?.DutyTitle,
                CareerStartDate = person.AstronautDetail?.CareerStartDate,
                CareerEndDate = person.AstronautDetail?.CareerEndDate
            };

            result.Person = personAstronaut;
            return result;
        }
    }

    public class GetPersonByNameResult : BaseResponse
    {
        public PersonAstronaut? Person { get; set; }
    }
}
