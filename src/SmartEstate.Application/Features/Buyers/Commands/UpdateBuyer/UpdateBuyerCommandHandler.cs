using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartEstate.Application.Common.Interfaces;
using SmartEstate.Application.Features.Buyers.DTOs;

namespace SmartEstate.Application.Features.Buyers.Commands.UpdateBuyer;

public class UpdateBuyerCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateBuyerCommand, ErrorOr<BuyerDto>>
{
    public async Task<ErrorOr<BuyerDto>> Handle(UpdateBuyerCommand request, CancellationToken cancellationToken)
    {
        var buyer = await db.Buyers
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

        if (buyer is null)
            return Error.NotFound("Buyer.NotFound", "Buyer not found.");

        buyer.FullName = request.FullName;
        buyer.LifestyleDescription = request.LifestyleDescription;
        buyer.Email = request.Email;
        buyer.Phone = request.Phone;
        buyer.BudgetMinEur = request.BudgetMinEur;
        buyer.BudgetMaxEur = request.BudgetMaxEur;
        buyer.PreferredLocations = request.PreferredLocations ?? [];

        await db.SaveChangesAsync(cancellationToken);

        return new BuyerDto
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
}
