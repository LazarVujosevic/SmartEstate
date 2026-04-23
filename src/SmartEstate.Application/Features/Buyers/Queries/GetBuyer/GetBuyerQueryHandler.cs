using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartEstate.Application.Common.Interfaces;
using SmartEstate.Application.Features.Buyers.DTOs;

namespace SmartEstate.Application.Features.Buyers.Queries.GetBuyer;

public class GetBuyerQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetBuyerQuery, ErrorOr<BuyerDto>>
{
    public async Task<ErrorOr<BuyerDto>> Handle(GetBuyerQuery request, CancellationToken cancellationToken)
    {
        var buyer = await db.Buyers
            .AsNoTracking()
            .Where(b => b.Id == request.Id)
            .Select(b => new BuyerDto
            {
                Id = b.Id,
                AssignedAgentId = b.AssignedAgentId,
                FullName = b.FullName,
                Email = b.Email,
                Phone = b.Phone,
                LifestyleDescription = b.LifestyleDescription,
                BudgetMinEur = b.BudgetMinEur,
                BudgetMaxEur = b.BudgetMaxEur,
                PreferredLocations = b.PreferredLocations,
                CreatedAt = b.CreatedAt,
                UpdatedAt = b.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (buyer is null)
            return Error.NotFound(description: "Buyer not found.");

        return buyer;
    }
}
