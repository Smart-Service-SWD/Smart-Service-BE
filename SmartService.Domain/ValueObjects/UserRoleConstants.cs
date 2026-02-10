namespace SmartService.Domain.ValueObjects;

/// <summary>
/// Constants for UserRole enum values to ensure consistent mapping
/// between enum values and authorization role strings.
/// </summary>
public static class UserRoleConstants
{
    /// <summary>
    /// Customer role - Khách hàng
    /// </summary>
    public const string Customer = "Customer";

    /// <summary>
    /// Staff role - Nhân viên quản lý
    /// </summary>
    public const string Staff = "Staff";

    /// <summary>
    /// Agent role - Nhà cung cấp dịch vụ
    /// </summary>
    public const string Agent = "Agent";

    /// <summary>
    /// Admin role - Quản trị viên
    /// </summary>
    public const string Admin = "Admin";

    /// <summary>
    /// All roles array for easy iteration
    /// </summary>
    public static string[] AllRoles => new[] { Customer, Staff, Agent, Admin };

    /// <summary>
    /// Staff and Admin roles (for authorization)
    /// Note: For use in [Authorize] attributes, use inline: new[] { Staff, Admin }
    /// </summary>
    public static string[] StaffAndAdmin => new[] { Staff, Admin };

    /// <summary>
    /// Agent, Staff and Admin roles (for authorization)
    /// Note: For use in [Authorize] attributes, use inline: new[] { Agent, Staff, Admin }
    /// </summary>
    public static string[] AgentStaffAndAdmin => new[] { Agent, Staff, Admin };
}
