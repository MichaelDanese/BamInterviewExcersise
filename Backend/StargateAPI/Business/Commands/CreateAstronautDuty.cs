using MediatR;
using MediatR.Pipeline;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Business.Extensions;
using StargateAPI.Business.Queries;
using StargateAPI.Business.Services.Interfaces;
using StargateAPI.Controllers;

namespace StargateAPI.Business.Commands
{
    public class CreateAstronautDuty : IRequest<CreateAstronautDutyResult>
    {
        public required string Name { get; set; }

        public required string Rank { get; set; }

        public required string DutyTitle { get; set; }

        public DateTime DutyStartDate { get; set; }
    }

    public class CreateAstronautDutyPreProcessor : IRequestPreProcessor<CreateAstronautDuty>
    {
        private readonly StarbaseContext _starbaseContext;

        public CreateAstronautDutyPreProcessor(StarbaseContext context)
        {
            _starbaseContext = context;
        }

        public async Task Process(CreateAstronautDuty request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new BadHttpRequestException("Person name cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(request.Rank))
            {
                throw new BadHttpRequestException("Rank cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(request.DutyTitle))
            {
                throw new BadHttpRequestException("Duty title cannot be empty");
            }

            var normalizedDutyTitle = request.DutyTitle.NormalizeNameOrTitle();
            var normalizedName = request.Name.NormalizeNameOrTitle().ToLower();

            var personId = await _starbaseContext.People
                .AsNoTracking()
                .Where(p => p.Name.ToLower() == normalizedName)
                .Select(p => p.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (personId == 0)
            {
                throw new BadHttpRequestException($"Cannot find person with name '{request.Name}'");
            }

            var hasPreviousConflictingDuty = await _starbaseContext.AstronautDuties
                .AsNoTracking()
                .AnyAsync(ad => 
                    ad.PersonId == personId &&
                    ad.DutyTitle == normalizedDutyTitle && 
                    ad.DutyStartDate == request.DutyStartDate, 
                    cancellationToken
                );

            if (hasPreviousConflictingDuty) {
                throw new BadHttpRequestException("Bad Request. Has conflicting duty.");
            }

            // check if this is a retirement duty
            // if so then we should ensure that this is not an immediate retirement
            // fulfill the rule "A Person's Career End Date is one day before the Retired Duty Start Dat" but also ensure that the person has at least one previous duty before retirement
            var isRetirement = normalizedDutyTitle.Equals("RETIRED", StringComparison.OrdinalIgnoreCase);
            if (isRetirement)
            {
                var hasPreviousDuty = await _starbaseContext.AstronautDuties
                    .AsNoTracking()
                    .AnyAsync(ad =>
                        ad.PersonId == personId &&
                        ad.DutyStartDate < request.DutyStartDate, 
                        cancellationToken
                    );

                if (!hasPreviousDuty)
                {
                    throw new BadHttpRequestException("Cannot retire without previous duty.");
                }

                var retireOnStartDate = await _starbaseContext.AstronautDuties
                    .AsNoTracking()
                    .AnyAsync(ad =>
                        ad.PersonId == personId &&
                        ad.DutyStartDate.Date == request.DutyStartDate.Date, 
                        cancellationToken
                    );

                if (retireOnStartDate)
                {
                    throw new BadHttpRequestException("Cannot retire on the same day career has started.");
                }
            }
        }
    }

    public class CreateAstronautDutyHandler : IRequestHandler<CreateAstronautDuty, CreateAstronautDutyResult>
    {
        private readonly StarbaseContext _starbaseContext;
        private readonly IMediator _mediator;
        private readonly IDatabaseLoggingService _logger;

        private const string RetiredDutyTitle = "RETIRED";

        public CreateAstronautDutyHandler(StarbaseContext context, IMediator mediator, IDatabaseLoggingService logger)
        {
            _starbaseContext = context;
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new astronaut duty assignment for a person.
        /// Updates the person's current astronaut details, closes any previous duty with an end date, and creates a new duty record.
        /// Enforces business rules: one current duty at a time, previous duty end dates set to day before new duty start, and retirement handling.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Result containing the ID of the newly created astronaut duty.</returns>
        /// <exception cref="BadHttpRequestException">Thrown if the person is not found.</exception>
        public async Task<CreateAstronautDutyResult> Handle(CreateAstronautDuty request, CancellationToken cancellationToken)
        {
            CreateAstronautDutyResult result = new CreateAstronautDutyResult();

            var normalizedDutyTitle = request.DutyTitle.NormalizeNameOrTitle();
            var normalizedRank = request.Rank.NormalizeNameOrTitle();
            var normalizedName = request.Name.NormalizeNameOrTitle();

            var isRetirement = normalizedDutyTitle.Equals(RetiredDutyTitle, StringComparison.OrdinalIgnoreCase);

            using (var transaction = await _starbaseContext.Database.BeginTransactionAsync(cancellationToken))
            {
                try
                {
                    var personResult = await _mediator.Send(new GetPersonByName { Name = normalizedName }, cancellationToken);
                    if (personResult?.Person is null)
                    {
                        throw new BadHttpRequestException($"Cannot find person with name '{normalizedName}'");
                    }

                    var person = personResult.Person;

                    var astronautDetail = await _starbaseContext.AstronautDetails
                        .FirstOrDefaultAsync(z => z.PersonId == person.PersonId, cancellationToken);

                    // insert
                    if (astronautDetail is null)
                    {
                        // a Person's Career End Date is one day before the Retired Duty Start Date.
                        DateTime? careerEndDate = isRetirement ?
                            request.DutyStartDate.AddDays(-1).Date :
                            null;

                        astronautDetail = new AstronautDetail()
                        {
                            PersonId = person.PersonId,
                            CurrentDutyTitle = normalizedDutyTitle,
                            CurrentRank = normalizedRank,
                            CareerStartDate = request.DutyStartDate.Date,
                            CareerEndDate = careerEndDate
                        };

                        await _starbaseContext.AstronautDetails.AddAsync(astronautDetail, cancellationToken);

                    }
                    // update
                    else
                    {
                        astronautDetail.CurrentDutyTitle = normalizedDutyTitle;
                        astronautDetail.CurrentRank = normalizedRank;

                        // update career start date if this duty starts earlier
                        if (request.DutyStartDate.Date < astronautDetail.CareerStartDate)
                        {
                            astronautDetail.CareerStartDate = request.DutyStartDate.Date;
                        }

                        if (isRetirement)
                        {
                            astronautDetail.CareerEndDate = request.DutyStartDate.AddDays(-1).Date;
                        }

                        _starbaseContext.AstronautDetails.Update(astronautDetail);
                    }

                    // close all open duties
                    // fulfills the rule "A Person will only ever hold one current Astronaut Duty Title, Start Date, and Rank at a time." 
                    var previousAstronautDuties = await _starbaseContext.AstronautDuties
                        .Where(ad => ad.PersonId == person.PersonId && ad.DutyEndDate == null)
                        .OrderByDescending(ad => ad.DutyStartDate)
                        .ToListAsync(cancellationToken);

                    if (previousAstronautDuties.Any())
                    {
                        foreach (var previousAstronautDuty in previousAstronautDuties)
                        {
                            previousAstronautDuty.DutyEndDate = request.DutyStartDate.AddDays(-1).Date;
                        }

                        _starbaseContext.AstronautDuties.UpdateRange(previousAstronautDuties);
                    }

                    var newAstronautDuty = new AstronautDuty()
                    {
                        PersonId = person.PersonId,
                        Rank = normalizedRank,
                        DutyTitle = normalizedDutyTitle,
                        DutyStartDate = request.DutyStartDate.Date,
                        DutyEndDate = null
                    };

                    await _starbaseContext.AstronautDuties.AddAsync(newAstronautDuty, cancellationToken);
                    await _starbaseContext.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    result = new CreateAstronautDutyResult()
                    {
                        Id = newAstronautDuty.Id
                    };
                }
                catch(Exception e)
                {
                    await transaction.RollbackAsync(cancellationToken);

                    await _logger.LogErrorAsync(
                        "Error when creating astronaut duty",
                        $"Failed to create astronaut duty for {normalizedName}: {normalizedDutyTitle} (Rank: {normalizedRank}) starting {request.DutyStartDate}",
                        e
                    );

                    throw;
                }
            }

            await _logger.LogInfoAsync(
                "Successfully created astronaut duty",
                $"Successfully created astronaut duty for {normalizedName}: {normalizedDutyTitle} (Rank: {normalizedRank}) starting {request.DutyStartDate}"
            );

            return result;
        }
    }

    public class CreateAstronautDutyResult : BaseResponse
    {
        public int? Id { get; set; }
    }
}
