using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using SmartService.Application.Features.Services.Commands.CreateServiceDefinition;
using SmartService.Application.Features.Services.Commands.UpdateServiceDefinition;
using SmartService.Application.Features.Services.Commands.DeleteServiceDefinition;

namespace SmartService.API.Controllers;

/// <summary>
/// Controller quản lý dịch vụ (Service Definitions) — chỉ dùng cho commands (Create/Update/Delete).
/// Queries dùng GraphQL: getServiceDefinitions, getServiceDefinitionById, getServiceDefinitionsByCategory.
/// </summary>
[ApiController]
[Route("api/services")]
[Tags("5. Services - Quản lý dịch vụ")]
public class ServicesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ServicesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// [CREATE] Tạo mới dịch vụ
    /// </summary>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Tạo mới dịch vụ",
        Description = "Tạo một dịch vụ mới trong hệ thống với thông tin: danh mục, tên, mô tả, giá cơ bản, thời gian ước tính. Sau khi tạo, KnowledgeBase JSON sẽ được đồng bộ tự động.",
        OperationId = "CreateServiceDefinition",
        Tags = new[] { "1. CREATE - Tạo mới" })]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateServiceDefinitionCommand command,
        CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Create), new { id }, id);
    }

    /// <summary>
    /// [UPDATE] Cập nhật thông tin dịch vụ
    /// </summary>
    [HttpPut("{id}")]
    [SwaggerOperation(
        Summary = "Cập nhật thông tin dịch vụ",
        Description = "Cập nhật tên, mô tả, giá, thời gian ước tính, trạng thái của dịch vụ. KnowledgeBase JSON sẽ được đồng bộ tự động.",
        OperationId = "UpdateServiceDefinition",
        Tags = new[] { "2. UPDATE - Cập nhật" })]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateServiceDefinitionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateServiceDefinitionCommand(
            id,
            request.Name,
            request.Description,
            request.BasePrice,
            request.EstimatedDuration,
            request.IsActive);

        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// [DELETE] Xóa dịch vụ
    /// </summary>
    [HttpDelete("{id}")]
    [SwaggerOperation(
        Summary = "Xóa dịch vụ",
        Description = "Xóa một dịch vụ khỏi hệ thống. KnowledgeBase JSON sẽ được đồng bộ tự động.",
        OperationId = "DeleteServiceDefinition",
        Tags = new[] { "3. DELETE - Xóa" })]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteServiceDefinitionCommand(id), cancellationToken);
        return NoContent();
    }
}

/// <summary>
/// Model request cập nhật dịch vụ
/// </summary>
public record UpdateServiceDefinitionRequest(
    string Name,
    string? Description,
    decimal BasePrice,
    int EstimatedDuration,
    bool IsActive);
