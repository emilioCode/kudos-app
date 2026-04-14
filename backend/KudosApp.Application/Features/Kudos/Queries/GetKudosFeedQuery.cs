using MediatR;
using KudosApp.Application.Features.Kudos.DTOs;

namespace KudosApp.Application.Features.Kudos.Queries;

public record GetKudosFeedQuery() : IRequest<List<KudosResponse>>;
