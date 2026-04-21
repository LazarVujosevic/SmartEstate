namespace SmartEstate.Application.Common.Interfaces;

public interface IAITaggingService
{
    Task<BuyerAIProfile> AnalyzeBuyerAsync(string lifestyleDescription, CancellationToken ct = default);
    Task<PropertyAIProfile> AnalyzePropertyAsync(string title, string description, CancellationToken ct = default);
}

public record BuyerAIProfile(List<string> Tags, string ProfileSummary);
public record PropertyAIProfile(List<string> Tags, string Summary);
