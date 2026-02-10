using SmartService.Domain.Entities;

namespace SmartService.Domain.ValueObjects;

/// <summary>
/// Extension methods for UserRole enum to facilitate mapping and conversion.
/// </summary>
public static class UserRoleExtensions
{
    /// <summary>
    /// Converts UserRole enum to authorization role string.
    /// This ensures consistent mapping between enum and authorization strings.
    /// </summary>
    /// <param name="role">The UserRole enum value</param>
    /// <returns>Role string for authorization (e.g., "Customer", "Staff", "Agent", "Admin")</returns>
    public static string ToAuthorizationRole(this UserRole role)
    {
        return role switch
        {
            UserRole.Customer => UserRoleConstants.Customer,
            UserRole.Staff => UserRoleConstants.Staff,
            UserRole.Agent => UserRoleConstants.Agent,
            UserRole.Admin => UserRoleConstants.Admin,
            _ => role.ToString() // Fallback to enum name
        };
    }

    /// <summary>
    /// Converts authorization role string to UserRole enum.
    /// </summary>
    /// <param name="roleString">The role string (e.g., "Customer", "Staff", "Agent", "Admin")</param>
    /// <returns>UserRole enum value</returns>
    /// <exception cref="ArgumentException">Thrown when role string is invalid</exception>
    public static UserRole FromAuthorizationRole(string roleString)
    {
        return roleString switch
        {
            UserRoleConstants.Customer => UserRole.Customer,
            UserRoleConstants.Staff => UserRole.Staff,
            UserRoleConstants.Agent => UserRole.Agent,
            UserRoleConstants.Admin => UserRole.Admin,
            _ => throw new ArgumentException($"Invalid role string: {roleString}", nameof(roleString))
        };
    }

    /// <summary>
    /// Checks if the role is a staff or admin role.
    /// </summary>
    /// <param name="role">The UserRole enum value</param>
    /// <returns>True if role is Staff or Admin</returns>
    public static bool IsStaffOrAdmin(this UserRole role)
    {
        return role == UserRole.Staff || role == UserRole.Admin;
    }

    /// <summary>
    /// Checks if the role is an agent, staff, or admin role.
    /// </summary>
    /// <param name="role">The UserRole enum value</param>
    /// <returns>True if role is Agent, Staff, or Admin</returns>
    public static bool IsAgentStaffOrAdmin(this UserRole role)
    {
        return role == UserRole.Agent || role == UserRole.Staff || role == UserRole.Admin;
    }
}
