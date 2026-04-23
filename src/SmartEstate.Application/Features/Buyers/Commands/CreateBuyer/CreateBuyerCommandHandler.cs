using ErrorOr;
using MediatR;
using SmartEstate.Application.Common.Interfaces;
using SmartEstate.Application.Features.Buyers.DTOs;
using SmartEstate.Domain.Entities;

namespace SmartEstate.Application.Features.Buyers.Commands.CreateBuyer;

public class CreateBuyerCommandHandler(IApplicationDbContext db, ITenantContext tenantContext)
    : IRequestHandler<CreateBuyerCommand, ErrorOr<BuyerDto>>
{
    public async Task<ErrorOr<BuyerDto>> Handle(CreateBuyerCommand request, CancellationToken cancellationToken)
    {
        var buyer = new Buyer
        {
            TenantId = tenantContext.TenantId!.Value,
            AssignedAgentId = request.AssignedAgentId,
            FullName = request.FullName,
            LifestyleDescription = request.LifestyleDescription,
            Email = request.Email,
            Phone = request.Phone,
            BudgetMinEur = request.BudgetMinEur,
            BudgetMaxEur = request.BudgetMaxEur,
            PreferredLocations = request.PreferredLocations ?? []
        };

        db.Buyers.Add(buyer);
        await db.SaveChangesAsync(cancellationToken);

        return MapToDto(buyer);
    }

    private static BuyerDto MapToDto(Buyer buyer) => new()
    {
        Id = buyer.Id,
        AssignedAgentId = buyer.AssignedAgentId,
        FullName = buyer.FullName,
        Email = buyer.Email,
        Phone = buyer.Phone,
        LifestyleDescription = buyer.LifestyleDescription,
        BudgetMinEur = buyer.BudgetMinEur,
        BudgetMaxEur = buyer.BudgetMaxEur,
        PreferredLocations = buyer.PreferredLocations,
        CreatedAt = buyer.CreatedAt,
        UpdatedAt = buyer.UpdatedAt
    };
}
