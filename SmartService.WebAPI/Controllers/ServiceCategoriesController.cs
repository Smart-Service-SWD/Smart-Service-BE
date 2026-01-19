using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using SmartService.Application.Features.ServiceCategories.Commands.Create;

namespace SmartService.API.Controllers;

/// <summary>
/// Controller quản lý danh mục dịch vụ (Service Categories)
/// </summary>
[ApiController]
[Route("api/service-categories")]
[Tags("4. Service Categories - Quản lý danh mục dịch vụ")]
public class ServiceCategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ServiceCategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// [CREATE] Tạo mới danh mục dịch vụ
    /// </summary>
    /// <param name="command">Thông tin danh mục dịch vụ cần tạo</param>
    /// <param name="cancellationToken">Token hủy</param>
    /// <returns>ID của danh mục dịch vụ vừa tạo</returns>
    /// <response code="201">Tạo danh mục dịch vụ thành công</response>
    /// <response code="400">Dữ liệu đầu vào không hợp lệ</response>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Tạo mới danh mục dịch vụ",
        Description = "Tạo một danh mục dịch vụ mới trong hệ thống với tên và mô tả",
        OperationId = "CreateServiceCategory",
        Tags = new[] { "1. CREATE - Tạo mới" })]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateServiceCategoryCommand command, CancellationToken cancellationToken)
    {
        var categoryId = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Create), new { id = categoryId }, categoryId);
    }
}
