using Fasally.Abstractions;

namespace Fasally.Errors;

public static class ProjectErrors
{
    public static readonly Error ProjectNotFound =
        new("Project.NotFound", "Project not found", StatusCodes.Status404NotFound);

    public static readonly Error InvalidStatusTransition =
        new("Project.InvalidStatusTransition", "Project status does not allow this action", StatusCodes.Status400BadRequest);

    public static readonly Error NotProjectTailor =
        new("Project.NotProjectTailor", "Only the project tailor can perform this action", StatusCodes.Status403Forbidden);
}
