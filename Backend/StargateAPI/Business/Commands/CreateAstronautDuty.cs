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
            if (request is null)
            {
                throw new BadHttpRequestException("Request cannot be null");
            }

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

            var normalizedDutyTitle = request.DutyTitle.NormalizeNameOrTitle().ToLower();
            var normalizedName = request.Name.NormalizeNameOrTitle().ToLower();
            var normalizedRank = request.Rank.NormalizeNameOrTitle().ToLower();
            var today = DateTime.UtcNow.Date;

            // prevent future dating
            if (request.DutyStartDate.Date > today)
            {
                throw new BadHttpRequestException("Duty start date cannot be in the future");
            }

            var personId = await _starbaseContext.People
                .AsNoTracking()
                .Where(p => p.Name.ToLower() == normalizedName)
                .Select(p => p.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (personId == 0)
            {
                throw new BadHttpRequestException($"Cannot find person with name '{request.Name}'");
            }

            // we cannot start two duties on the same date
            var existingDutySameStartDate = await _starbaseContext.AstronautDuties
                .AsNoTracking()
                .AnyAsync(ad =>
                    ad.PersonId == personId &&
                    ad.DutyStartDate.Date == request.DutyStartDate.Date,
                    cancellationToken
                );

            if (existingDutySameStartDate)
            {
                throw new BadHttpRequestException("Cannot start two duties on the same date for the same person.");
            }

            // if the duty falls in the bounds of an existing duty we can adjust those bounds but only if the rank and title are different
            // this cleanly handles backdating
            var sameRankAndTitleInBounds = await _starbaseContext.AstronautDuties
            .AsNoTracking()
            .AnyAsync(ad =>
                ad.PersonId == personId &&
                ad.DutyStartDate.Date <= request.DutyStartDate.Date &&
                ad.DutyEndDate.HasValue && ad.DutyEndDate.Value.Date >= request.DutyStartDate.Date &&
                ad.Rank.ToLower() == normalizedRank &&
                ad.DutyTitle.ToLower() == normalizedDutyTitle,
                cancellationToken
            );

            if (sameRankAndTitleInBounds)
            {
                throw new BadHttpRequestException("Cannot create a duty with the same rank and title within the bounds of an existing duty.");
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
                    var personId = await _starbaseContext.People
                        .AsNoTracking()
                        .Where(p => p.Name.ToLower() == normalizedName.ToLower())
                        .Select(p => p.Id)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (personId == 0)
                    {
                        throw new BadHttpRequestException($"Person with name '{normalizedName}' not found.");
                    }

                    var astronautDetail = await _starbaseContext.AstronautDetails
                        .FirstOrDefaultAsync(z => z.PersonId == personId, cancellationToken);

                    var nextDutyStartDate = await _starbaseContext.AstronautDuties
                        .Where(ad => ad.PersonId == personId && ad.DutyStartDate.Date > request.DutyStartDate.Date)
                        .OrderBy(ad => ad.DutyStartDate)
                        .Select(ad => (DateTime?)ad.DutyStartDate)
                        .FirstOrDefaultAsync(cancellationToken);

                    var isBackdated = nextDutyStartDate.HasValue;

                    // insert
                    if (astronautDetail is null)
                    {
                        astronautDetail = new AstronautDetail()
                        {
                            PersonId = personId,
                            CareerStartDate = request.DutyStartDate.Date
                        };

                        await _starbaseContext.AstronautDetails.AddAsync(astronautDetail, cancellationToken);
                    }
                    // update
                    else
                    {
                        // update career start date if this duty starts earlier
                        if (request.DutyStartDate.Date < astronautDetail.CareerStartDate)
                        {
                            astronautDetail.CareerStartDate = request.DutyStartDate.Date;
                        }

                        // update if person comes out of retirement
                        if (astronautDetail.CareerEndDate.HasValue && request.DutyStartDate.Date > astronautDetail.CareerEndDate.Value)
                        {
                            astronautDetail.CareerEndDate = null;
                        }

                        if (isRetirement && !isBackdated)
                        {
                            astronautDetail.CareerEndDate = request.DutyStartDate.AddDays(-1).Date;
                        }

                        _starbaseContext.AstronautDetails.Update(astronautDetail);
                    }

                    var previousDuty = await _starbaseContext.AstronautDuties
                        .Where(ad =>
                            ad.PersonId == personId &&
                            ad.DutyStartDate.Date < request.DutyStartDate.Date &&
                            !ad.DutyEndDate.HasValue)
                        .SingleOrDefaultAsync(cancellationToken);

                    if (previousDuty != null)
                    {
                        previousDuty.DutyEndDate = request.DutyStartDate.AddDays(-1).Date;
                        _starbaseContext.AstronautDuties.Update(previousDuty);
                    }

                    // if the new duty falls in the bounds of another duty, we need to adjust the end date of that duty to be the day before the new duty start date
                    var conflictingDuties = await _starbaseContext.AstronautDuties
                        .Where(ad =>
                            ad.PersonId == personId &&
                            ad.DutyStartDate.Date < request.DutyStartDate.Date &&
                            ad.DutyEndDate.HasValue && ad.DutyEndDate.Value.Date >= request.DutyStartDate.Date)
                        .SingleOrDefaultAsync(cancellationToken);

                    if (conflictingDuties != null)
                    {
                        conflictingDuties.DutyEndDate = request.DutyStartDate.AddDays(-1).Date;
                        _starbaseContext.AstronautDuties.Update(conflictingDuties);
                    }

                    var newAstronautDuty = new AstronautDuty()
                    {
                        PersonId = personId,
                        Rank = normalizedRank,
                        DutyTitle = normalizedDutyTitle,
                        DutyStartDate = request.DutyStartDate.Date,
                        DutyEndDate = nextDutyStartDate?.AddDays(-1).Date
                    };

                    await _starbaseContext.AstronautDuties.AddAsync(newAstronautDuty, cancellationToken);
                    await _starbaseContext.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    result = new CreateAstronautDutyResult()
                    {
                        Id = newAstronautDuty.Id,
                        Message = $"Successfully created astronaut duty for {normalizedName}: {normalizedDutyTitle} (Rank: {normalizedRank}) starting {request.DutyStartDate:MM-dd-yyyy}"
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
