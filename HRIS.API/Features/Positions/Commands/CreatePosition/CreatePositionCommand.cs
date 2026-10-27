
using HRIS.API.Data.Repositories;
using HRIS.API.DTOs.Positions;
using HRIS.API.Entities;
using MediatR;

namespace HRIS.API.Features.Positions.Commands.CreatePosition;

public record CreatePositionCommand(
    CreatePositionRequest Request
) : IRequest<Guid>;

public class CreatePositionCommandHandler(IUnitOfWork uow)
    : IRequestHandler<CreatePositionCommand, Guid>
{
    public async Task<Guid> Handle(
        CreatePositionCommand command,
        CancellationToken ct)
    {
        var request = command.Request;

        var position = new Position
        {
            Id = Guid.CreateVersion7(),
            Title = request.Title,
            Level = request.Level
        };

        await uow.Positions.AddAsync(position, ct);
        await uow.SaveChangesAsync(ct);

        return position.Id;
    }
}
