namespace SmartService.Domain.Exceptions
{
    // ======================== AUTH-RELATED DOMAIN EXCEPTIONS =========================

    /// <summary>
    /// Exception for Authentication-related domain issues.
    /// </summary>
    public abstract class AuthException : DomainException
    {
        protected AuthException(string message) : base(message) { }

        public class EmailAlreadyRegisteredException : AuthException
        {
            public EmailAlreadyRegisteredException()
                : base("Email already registered.") { }
        }

        public class InvalidRoleException : AuthException
        {
            public InvalidRoleException()
                : base("Invalid user role.") { }
        }

        public class InvalidCredentialsException : AuthException
        {
            public InvalidCredentialsException()
                : base("Invalid email or password.") { }
        }

        public class UserNotFoundException : AuthException
        {
            public UserNotFoundException()
                : base("User not found.") { }
        }

        public class InvalidRefreshTokenException : AuthException
        {
            public InvalidRefreshTokenException()
                : base("Invalid or expired refresh token.") { }
        }
    }
}
