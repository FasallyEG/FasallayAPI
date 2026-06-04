namespace Fasally.Contracts.Proposals;

public class CreateProposalRequest
{
    public string TailorId { get; set; } = default!;
    public string Description { get; set; } = string.Empty;
    public DateTime? ResponseDeadline { get; set; }
    public List<string> ImageUrls { get; set; } = [];
}
