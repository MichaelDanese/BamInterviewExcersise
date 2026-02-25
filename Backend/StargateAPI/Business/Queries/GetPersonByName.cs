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

            var personAstronaut = await _starbaseContext.People
                    .AsNoTracking()
                    .Where(p => p.Name.ToLower() == normalizedName)
                    .Select(p => new PersonAstronaut
                    {
                        PersonId = p.Id,
                        Name = p.Name,
                        CurrentRank = p.AstronautDetail != null ? p.AstronautDetail.CurrentRank : null,
                        CurrentDutyTitle = p.AstronautDetail != null ? p.AstronautDetail.CurrentDutyTitle : null,
                        CareerStartDate = p.AstronautDetail != null ? p.AstronautDetail.CareerStartDate : null,
                        CareerEndDate = p.AstronautDetail != null ? p.AstronautDetail.CareerEndDate : null
                    })
                    .FirstOrDefaultAsync(cancellationToken);

            result.Person = personAstronaut;
            return result;
        }
    }

    public class GetPersonByNameResult : BaseResponse
    {
        public PersonAstronaut? Person { get; set; }
    }
}
