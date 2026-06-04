namespace Fasally.Entities;

public class ProposalImage
{
    public Guid Id { get; set; }

    public Guid ProposalId { get; set; }
    public Proposal Proposal { get; set; } = default!;

    public string ImageUrl { get; set; } = string.Empty;
}
