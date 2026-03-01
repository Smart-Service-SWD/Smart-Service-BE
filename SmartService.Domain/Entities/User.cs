namespace SmartService.Domain.Entities;

/// <summary>
/// Represents a domain-level user in the system.
/// 
/// This entity contains business-related user information
/// and role classification, but does NOT manage authentication
/// or authorization concerns.
/// 
/// A User may act in one or more business roles such as:
/// - Customer: creates service requests
/// - Staff: evaluates complexity and coordinates services
/// - Service Agent: executes assigned requests
/// - Administrator: manages system configurations
/// </summary>
public class User
{
    public Guid Id { get; private set; }
    public string FullName { get; private set; }
    public string Email { get; private set; }
    public string PhoneNumber { get; private set; }
    public UserRole Role { get; private set; }

    private User() { }

    private User(string fullName, string email, string phoneNumber, UserRole role)
    {
        Id = Guid.NewGuid();
        FullName = fullName;
        Email = email;
        PhoneNumber = phoneNumber;
        Role = role;
    }

    public static User CreateCustomer(string fullName, string email, string phone)
        => new(fullName, email, phone, UserRole.Customer);

    public static User CreateStaff(string fullName, string email, string phone)
        => new(fullName, email, phone, UserRole.Staff);

    public static User CreateAgent(string fullName, string email, string phone)
        => new(fullName, email, phone, UserRole.Agent);

    public void ChangeRole(UserRole role)
    {
        Role = role;
    }

    public void UpdateProfile(string fullName, string phoneNumber)
    {
        FullName = fullName;
        PhoneNumber = phoneNumber;
    }
}

public enum UserRole
{
    Customer = 0,
    Staff = 1,
    Agent = 2,
    Admin = 3
}
