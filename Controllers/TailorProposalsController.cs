using Fasally.Abstractions;
using Fasally.Abstractions.Consts;
using Fasally.Authentication.Filters;
using Fasally.Contracts.Proposals;
using Fasally.Extensions;
using Fasally.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fasally.Controllers;

[Route("api/tailor/proposals")]
[ApiController]
[Authorize]
public class TailorProposalsController(ITailorProposalService tailorProposalService) : ControllerBase
{
    private readonly ITailorProposalService _tailorProposalService = tailorProposalService;

    [HttpGet("")]
    [HasPermission(Permissions.ViewTailorProposals)]
    public async Task<IActionResult> GetTailorProposals(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId()!;
        var result = await _tailorProposalService.GetTailorProposalsAsync(userId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("{proposalId:guid}/accept")]
    [HasPermission(Permissions.AcceptProposal)]
    public async Task<IActionResult> AcceptProposal(
        [FromRoute] Guid proposalId,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId()!;
        var result = await _tailorProposalService.AcceptProposalAsync(userId, proposalId, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpPost("{proposalId:guid}/reject")]
    [HasPermission(Permissions.RejectTailorProposal)]
    public async Task<IActionResult> RejectProposal(
        [FromRoute] Guid proposalId,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId()!;
        var result = await _tailorProposalService.RejectProposalAsync(userId, proposalId, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpPost("{proposalId:guid}/finalize")]
    [HasPermission(Permissions.FinalizeProposal)]
    public async Task<IActionResult> FinalizeProposal(
        [FromRoute] Guid proposalId,
        [FromBody] FinalizeProposalRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId()!;
        var result = await _tailorProposalService.FinalizeProposalAsync(userId, proposalId, request, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }
}
