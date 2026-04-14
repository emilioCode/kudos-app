using MediatR;
using KudosApp.Application.Features.Kudos.DTOs;

namespace KudosApp.Application.Features.Kudos.Commands;

public record GiveKudosCommand(
    Guid SenderId,
    Guid RecipientId,
    Guid CategoryId,
    string Message
) : IRequest<KudosResponse>;
