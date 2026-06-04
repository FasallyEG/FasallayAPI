using Fasally.Abstractions;

namespace Fasally.Errors;

public static class ProposalErrors
{
    public static readonly Error ProposalNotFound =
        new("Proposal.NotFound", "Proposal not found", StatusCodes.Status404NotFound);

    public static readonly Error InvalidStatusTransition =
        new("Proposal.InvalidStatusTransition", "Proposal status does not allow this action", StatusCodes.Status400BadRequest);

    public static readonly Error ProductNotFound =
        new("Proposal.ProductNotFound", "One or more proposal products were not found", StatusCodes.Status404NotFound);

    public static readonly Error DuplicateProduct =
        new("Proposal.DuplicateProduct", "Duplicate products are not allowed in the same proposal", StatusCodes.Status400BadRequest);

    public static readonly Error NotProposalClient =
        new("Proposal.NotProposalClient", "Only the proposal client can perform this action", StatusCodes.Status403Forbidden);

    public static readonly Error NotProposalTailor =
        new("Proposal.NotProposalTailor", "Only the proposal tailor can perform this action", StatusCodes.Status403Forbidden);
}
