using Fasally.Abstractions;
using Fasally.Abstractions.Consts;
using Fasally.Authentication.Filters;
using Fasally.Contracts.Proposals;
using Fasally.Extensions;
using Fasally.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fasally.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ProposalsController(IProposalService proposalService) : ControllerBase
{
    private readonly IProposalService _proposalService = proposalService;

    [HttpGet("")]
    [HasPermission(Permissions.ViewMyProposals)]
    public async Task<IActionResult> GetMyProposals(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId()!;
        var result = await _proposalService.GetMyProposalsAsync(userId, cancellationToken);
        return Ok(result.Value);
    }

    [HttpGet("{proposalId:guid}")]
    [HasPermission(Permissions.ViewMyProposals)]
    public async Task<IActionResult> GetProposalDetails(
        [FromRoute] Guid proposalId,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId()!;
        var result = await _proposalService.GetProposalDetailsAsync(userId, proposalId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("")]
    [HasPermission(Permissions.CreateProposal)]
    public async Task<IActionResult> CreateProposal(
        [FromBody] CreateProposalRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId()!;
        var result = await _proposalService.CreateProposalAsync(userId, request, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetProposalDetails), new { proposalId = result.Value }, result.Value)
            : result.ToProblem();
    }

    [HttpPost("{proposalId:guid}/approve")]
    [HasPermission(Permissions.ApproveProposal)]
    public async Task<IActionResult> ApproveProposal(
        [FromRoute] Guid proposalId,
        [FromBody] ApproveProposalRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId()!;
        var result = await _proposalService.ApproveProposalAsync(userId, proposalId, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("{proposalId:guid}/reject")]
    [HasPermission(Permissions.RejectProposal)]
    public async Task<IActionResult> RejectProposal(
        [FromRoute] Guid proposalId,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId()!;
        var result = await _proposalService.RejectProposalAsync(userId, proposalId, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }
}
