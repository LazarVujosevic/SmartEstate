using ErrorOr;
using MediatR;
using SmartEstate.Application.Features.Buyers.DTOs;

namespace SmartEstate.Application.Features.Buyers.Commands.UpdateBuyer;

public record UpdateBuyerCommand(
    Guid Id,
    string FullName,
    string LifestyleDescription,
    string? Email,
    string? Phone,
    decimal? BudgetMinEur,
    decimal? BudgetMaxEur,
    List<string>? PreferredLocations) : IRequest<ErrorOr<BuyerDto>>;
