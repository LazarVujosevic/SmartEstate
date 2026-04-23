using ErrorOr;
using MediatR;

namespace SmartEstate.Application.Features.Buyers.Commands.DeleteBuyer;

public record DeleteBuyerCommand(Guid Id) : IRequest<ErrorOr<Deleted>>;
