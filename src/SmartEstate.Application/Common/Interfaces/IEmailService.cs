namespace SmartEstate.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default);
    Task SendToManyAsync(IEnumerable<string> recipients, string subject, string htmlBody, CancellationToken ct = default);
}
