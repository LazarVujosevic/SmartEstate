using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartEstate.Application.Common.Interfaces;

namespace SmartEstate.Application.Features.Buyers.Commands.DeleteBuyer;

public class DeleteBuyerCommandHandler(IApplicationDbContext db)
    : IRequestHandler<DeleteBuyerCommand, ErrorOr<Deleted>>
{
    public async Task<ErrorOr<Deleted>> Handle(DeleteBuyerCommand request, CancellationToken cancellationToken)
    {
        var buyer = await db.Buyers
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

        if (buyer is null)
            return Error.NotFound("Buyer.NotFound", "Buyer not found.");

        buyer.IsDeleted = true;
        await db.SaveChangesAsync(cancellationToken);

        return Result.Deleted;
    }
}
