using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using SmartService.Application.Features.Auth.Commands.Login;
using SmartService.Application.Features.Auth.Commands.Logout;
using SmartService.Application.Features.Auth.Commands.RefreshToken;
using SmartService.Application.Features.Auth.Commands.Register;
using SmartService.Application.Features.Auth.Commands.UpdateProfile;
using SmartService.Application.Features.Auth.Commands.UpdateUserRole;
using SmartService.Domain.Entities;
using SmartService.Domain.Exceptions;
using SmartService.Domain.ValueObjects;
using System.Security.Claims;


namespace SmartService.API.Controllers;

/// <summary>
/// Controller quản lý authentication (Đăng ký, Đăng nhập, Refresh Token, Đăng xuất)
/// </summary>
[ApiController]
[Route("api/auth")]
[Tags("0. Authentication - Xác thực người dùng")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// [POST] Đăng ký tài khoản mới
    /// </summary>
    /// <param name="request">Thông tin đăng ký</param>
    /// <param name="cancellationToken">Token hủy</param>
    /// <returns>Kết quả authentication với access token và refresh token</returns>
    /// <response code="200">Đăng ký thành công</response>
    /// <response code="400">Dữ liệu đầu vào không hợp lệ hoặc email đã tồn tại</response>
    [HttpPost("register")]
    [SwaggerOperation(
        Summary = "Đăng ký tài khoản mới",
        Description = "Đăng ký một tài khoản mới trong hệ thống và nhận authentication tokens",
        OperationId = "Register",
        Tags = new[] { "0. Authentication - Xác thực người dùng" })]
    [ProducesResponseType(typeof(Application.Abstractions.Auth.AuthResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RegisterCommand(
            Email: request.Email,
            Password: request.Password,
            FullName: request.FullName,
            PhoneNumber: request.PhoneNumber,
            Role: UserRole.Customer
        );

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// [POST] Đăng nhập
    /// </summary>
    /// <param name="request">Thông tin đăng nhập</param>
    /// <param name="cancellationToken">Token hủy</param>
    /// <returns>Kết quả authentication với access token và refresh token</returns>
    /// <response code="200">Đăng nhập thành công</response>
    /// <response code="400">Dữ liệu đầu vào không hợp lệ</response>
    /// <response code="401">Email hoặc mật khẩu không đúng</response>
    [HttpPost("login")]
    [SwaggerOperation(
        Summary = "Đăng nhập",
        Description = "Đăng nhập với email và mật khẩu để nhận authentication tokens",
        OperationId = "Login",
        Tags = new[] { "0. Authentication - Xác thực người dùng" })]
    [ProducesResponseType(typeof(Application.Abstractions.Auth.AuthResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var command = new LoginCommand(
            Email: request.Email,
            Password: request.Password
        );

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// [POST] Làm mới access token
    /// </summary>
    /// <param name="request">Refresh token</param>
    /// <param name="cancellationToken">Token hủy</param>
    /// <returns>Access token và refresh token mới</returns>
    /// <response code="200">Refresh token thành công</response>
    /// <response code="400">Refresh token không hợp lệ hoặc đã hết hạn</response>
    [HttpPost("refresh-token")]
    [SwaggerOperation(
        Summary = "Làm mới access token",
        Description = "Sử dụng refresh token để lấy access token mới",
        OperationId = "RefreshToken",
        Tags = new[] { "0. Authentication - Xác thực người dùng" })]
    [ProducesResponseType(typeof(Application.Abstractions.Auth.AuthResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RefreshTokenCommand(
            RefreshToken: request.RefreshToken
        );

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// [POST] Đăng xuất
    /// </summary>
    /// <param name="request">Refresh token cần revoke</param>
    /// <param name="cancellationToken">Token hủy</param>
    /// <returns>Kết quả đăng xuất</returns>
    /// <response code="200">Đăng xuất thành công</response>
    /// <response code="400">Refresh token không hợp lệ</response>
    [HttpPost("logout")]
    [SwaggerOperation(
        Summary = "Đăng xuất",
        Description = "Revoke refresh token để đăng xuất người dùng",
        OperationId = "Logout",
        Tags = new[] { "0. Authentication - Xác thực người dùng" })]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutRequest request,
        CancellationToken cancellationToken)
    {
        var command = new LogoutCommand(
            RefreshToken: request.RefreshToken
        );

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// [PATCH] Cập nhật vai trò (role) của tài khoản người dùng.
    /// </summary>
    /// <remarks>
    /// - Admin có thể cập nhật sang bất kỳ role nào (Customer, Staff, Agent, Admin).<br/>
    /// - Staff chỉ được phép cập nhật role của user khác sang Agent.<br/>
    /// - Dùng để nâng cấp tài khoản Customer thành Staff/Agent/Admin theo phân quyền.
    /// </remarks>
    [HttpPatch("users/{id:guid}/role")]
    [Authorize(Roles = $"{UserRoleConstants.Admin},{UserRoleConstants.Staff}")]
    [SwaggerOperation(
        Summary = "Cập nhật vai trò tài khoản",
        Description = "Admin/Staff cập nhật vai trò (role) cho tài khoản người dùng. Staff chỉ được phép nâng cấp lên Agent.",
        OperationId = "UpdateUserRole",
        Tags = new[] { "0. Authentication - Xác thực người dùng" })]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateUserRole(
        [FromRoute] Guid id,
        [FromBody] UpdateUserRoleRequest request,
        CancellationToken cancellationToken)
    {
        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
        if (string.IsNullOrWhiteSpace(roleClaim))
        {
            throw new UnauthorizedAccessException();
        }

        var currentUserRole = UserRoleExtensions.FromAuthorizationRole(roleClaim);

        if (currentUserRole is not (UserRole.Admin or UserRole.Staff))
        {
            throw new UnauthorizedAccessException();
        }

        if (!TryParseUserRole(request.Role, out var newRole))
        {
            throw new AuthException.InvalidRoleException();
        }

        if (currentUserRole == UserRole.Staff && newRole != UserRole.Agent)
        {
            throw new BusinessRuleException.BusinessConstraintViolationException("Role", "Staff is only allowed to update role to Agent.");
        }

        var command = new UpdateUserRoleCommand(id, newRole);
        var success = await _mediator.Send(command, cancellationToken);

        if (!success)
        {
            throw new AuthException.UserNotFoundException();
        }

        return Ok(true);
    }

    /// <summary>
    /// [PUT] Cập nhật hồ sơ cá nhân
    /// </summary>
    /// <param name="request">Thông tin cần cập nhật</param>
    /// <param name="cancellationToken">Token hủy</param>
    /// <returns>True nếu cập nhật thành công</returns>
    [HttpPut("profile")]
    [Authorize]
    [SwaggerOperation(
        Summary = "Cập nhật hồ sơ cá nhân",
        Description = "Cập nhật họ tên, email, số điện thoại của người dùng đang đăng nhập",
        OperationId = "UpdateProfile",
        Tags = new[] { "0. Authentication - Xác thực người dùng" })]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException();
        }

        var command = new UpdateProfileCommand(
            UserId: userId,
            FullName: request.FullName,
            PhoneNumber: request.PhoneNumber
        );

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    private static bool TryParseUserRole(string roleInput, out UserRole role)
    {
        role = UserRole.Customer;
        if (string.IsNullOrWhiteSpace(roleInput)) return false;

        var input = roleInput.Trim();
        if (int.TryParse(input, out var num) && Enum.IsDefined(typeof(UserRole), num))
        {
            role = (UserRole)num;
            return true;
        }

        return Enum.TryParse(input, ignoreCase: true, out role);
    }
}

/// <summary>
/// Request model cho đăng ký
/// </summary>
/// <param name="Email">Email của người dùng</param>
/// <param name="Password">Mật khẩu (tối thiểu 6 ký tự)</param>
/// <param name="FullName">Họ và tên</param>
/// <param name="PhoneNumber">Số điện thoại</param>
public record RegisterRequest(
    [SwaggerParameter(Description = "Email của người dùng")] string Email,
    [SwaggerParameter(Description = "Mật khẩu (tối thiểu 6 ký tự)")] string Password,
    [SwaggerParameter(Description = "Họ và tên")] string FullName,
    [SwaggerParameter(Description = "Số điện thoại")] string PhoneNumber);

/// <summary>
/// Request model cho đăng nhập
/// </summary>
public record LoginRequest(
    string Email,
    string Password);

/// <summary>
/// Request model cho refresh token
/// </summary>
public record RefreshTokenRequest(
    string RefreshToken);

/// <summary>
/// Request model cho logout
/// </summary>
public record LogoutRequest(
    string RefreshToken);

/// <summary>
/// Request model cho cập nhật vai trò tài khoản.
/// </summary>
public record UpdateUserRoleRequest(
    [SwaggerParameter(Description = "Vai trò mới: 'Customer' hoặc 0, 'Staff' hoặc 1, 'Agent' hoặc 2, 'Admin' hoặc 3")]
    string Role);

/// <summary>
/// Request model cho cập nhật hồ sơ cá nhân.
/// </summary>
public record UpdateProfileRequest(
    [SwaggerParameter(Description = "Họ và tên")] string FullName,
    [SwaggerParameter(Description = "Số điện thoại")] string PhoneNumber);
