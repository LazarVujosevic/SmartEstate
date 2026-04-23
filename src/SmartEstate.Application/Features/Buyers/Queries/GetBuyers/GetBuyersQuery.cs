using ErrorOr;
using MediatR;
using SmartEstate.Application.Common.Models;
using SmartEstate.Application.Features.Buyers.DTOs;

namespace SmartEstate.Application.Features.Buyers.Queries.GetBuyers;

public record GetBuyersQuery(
    int PageNumber,
    int PageSize,
    string? Search) : IRequest<ErrorOr<PagedResult<BuyerListItemDto>>>;
