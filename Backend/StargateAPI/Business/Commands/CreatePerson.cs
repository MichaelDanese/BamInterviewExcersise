using MediatR;
using MediatR.Pipeline;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
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
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new BadHttpRequestException("Bad Request. Name is required");
            }

            var normalizedName = request.Name.ToLower().Trim();
            var personExists = await _context.People.AsNoTracking()
                .AnyAsync(z => z.Name.ToLower().Trim() == normalizedName, 
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

        public CreatePersonHandler(StarbaseContext context)
        {
            _context = context;
        }
        public async Task<CreatePersonResult> Handle(CreatePerson request, CancellationToken cancellationToken)
        {
            var normalizedName = request.Name.Trim();

            var newPerson = new Person()
            {
                Name = normalizedName
            };

            await _context.People.AddAsync(newPerson, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);
            return new CreatePersonResult()
            {
                Id = newPerson.Id
            };
        }
    }

    public class CreatePersonResult : BaseResponse
    {
        public int Id { get; set; }
    }
}
