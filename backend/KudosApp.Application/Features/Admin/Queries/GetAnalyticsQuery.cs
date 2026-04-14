using MediatR;
using KudosApp.Application.Features.Admin.DTOs;

namespace KudosApp.Application.Features.Admin.Queries;

public record GetAnalyticsQuery() : IRequest<AnalyticsResponse>;
