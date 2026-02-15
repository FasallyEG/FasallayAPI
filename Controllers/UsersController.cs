
using Fasally.Abstractions;
using Fasally.Contracts.Users;
using Fasally.Services;
using Microsoft.AspNetCore.Mvc;

namespace Fasally.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController(IUserService userService):ControllerBase
    {
    private readonly IUserService _userService = userService;

    [HttpGet("")]
    public async Task<IActionResult> GetAllsAsync(CancellationToken cancellationToken = default)
        {
        return Ok(await _userService.GetAllsAsync(cancellationToken));
        }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get([FromRoute] string id)
        {
        var result = await _userService.GetAsync(id);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
        }

    [HttpPost("")]
    public async Task<IActionResult> Add([FromBody] CreateUserRequest request,CancellationToken cancellationToken)
        {
        var result = await _userService.AddAsync(request,cancellationToken);

        return result.IsSuccess ? CreatedAtAction(nameof(Get),new { result.Value.Id },result.Value) : result.ToProblem();
        }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id,[FromBody] UpdateUserRequest request,CancellationToken cancellationToken)
        {
        var result = await _userService.UpdateAsync(id,request,cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblem();
        }

    [HttpPut("toggle-status/{id}")]
    public async Task<IActionResult> Toggle(string id)
        {
        var result = await _userService.ToggleStatusAsync(id);

        return result.IsSuccess ? NoContent() : result.ToProblem();
        }

    [HttpPut("unlock-user/{id}")]
    public async Task<IActionResult> UnlockUser(string id)
        {
        var result = await _userService.UnlockUser(id);

        return result.IsSuccess ? NoContent() : result.ToProblem();
        }
    }
