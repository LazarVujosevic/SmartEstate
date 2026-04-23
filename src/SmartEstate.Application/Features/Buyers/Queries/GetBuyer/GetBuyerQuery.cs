using ErrorOr;
using MediatR;
using SmartEstate.Application.Features.Buyers.DTOs;

namespace SmartEstate.Application.Features.Buyers.Queries.GetBuyer;

public record GetBuyerQuery(Guid Id) : IRequest<ErrorOr<BuyerDto>>;
