using Microsoft.AspNetCore.Authorization;

namespace Fasally.Authentication.Filters;

public class HasPermissionAttribute(string permission) : AuthorizeAttribute(permission)
    {
    }
