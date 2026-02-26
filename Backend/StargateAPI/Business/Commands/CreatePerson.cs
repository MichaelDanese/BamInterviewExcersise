using MediatR;
using MediatR.Pipeline;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Business.Extensions;
using StargateAPI.Business.Services.Interfaces;
using StargateAPI.Controllers;

namespace StargateAPI.Business.Commands
{
    public class CreatePerson : IRequest<CreatePersonResult>
    {
        public required string Name { get; set; } = string.Empty;
    }

    public class CreatePersonPreProcessor : IRequestPreProcessor<CreatePerson>
    {
        private readonly StarbaseContext _context;
        
        public CreatePersonPreProcessor(StarbaseContext context)
        {
            _context = context;
        }

        public async Task Process(CreatePerson request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request?.Name))
            {
                throw new BadHttpRequestException("Bad Request. Name is required");
            }

            var normalizedName = request.Name.ToLower().NormalizeNameOrTitle();
            var personExists = await _context.People.AsNoTracking()
                .AnyAsync(z => z.Name.ToLower() == normalizedName, 
                cancellationToken);

            if (personExists)
            {
                throw new BadHttpRequestException("Bad Request. Name already exists in system");
            }
        }
    }

    public class CreatePersonHandler : IRequestHandler<CreatePerson, CreatePersonResult>
    {
        private readonly StarbaseContext _context;
        private readonly IDatabaseLoggingService _logger;

        public CreatePersonHandler(StarbaseContext context, IDatabaseLoggingService logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<CreatePersonResult> Handle(CreatePerson request, CancellationToken cancellationToken)
        {
            var normalizedName = request.Name.NormalizeNameOrTitle();

            var newPerson = new Person()
            {
                Name = normalizedName
            };

            await _context.People.AddAsync(newPerson, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            await _logger.LogInfoAsync(
                "Successfully created person",
                $"Successfully created person with name {normalizedName}"
            );

            return new CreatePersonResult()
            {
                Id = newPerson.Id,
                Message = $"Person with the name of {newPerson.Name} created successfully"
            };
        }
    }

    public class CreatePersonResult : BaseResponse
    {
        public int Id { get; set; }
    }
}
