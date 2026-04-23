using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartEstate.Application.Common.Interfaces;
using SmartEstate.Application.Common.Models;
using SmartEstate.Application.Features.Buyers.DTOs;

namespace SmartEstate.Application.Features.Buyers.Queries.GetBuyers;

public class GetBuyersQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetBuyersQuery, ErrorOr<PagedResult<BuyerListItemDto>>>
{
    public async Task<ErrorOr<PagedResult<BuyerListItemDto>>> Handle(
        GetBuyersQuery request, CancellationToken cancellationToken)
    {
        var query = db.Buyers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(b =>
                b.FullName.ToLower().Contains(search) ||
                (b.Email != null && b.Email.ToLower().Contains(search)) ||
                (b.Phone != null && b.Phone.ToLower().Contains(search)) ||
                b.PreferredLocations.Any(l => l.ToLower().Contains(search)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(b => new BuyerListItemDto
            {
                Id = b.Id,
                FullName = b.FullName,
                Email = b.Email,
                Phone = b.Phone,
                BudgetMinEur = b.BudgetMinEur,
                BudgetMaxEur = b.BudgetMaxEur,
                PreferredLocations = b.PreferredLocations,
                CreatedAt = b.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<BuyerListItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
