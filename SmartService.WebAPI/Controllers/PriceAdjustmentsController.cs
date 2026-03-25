using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using SmartService.Application.Features.PriceAdjustments.Commands.ApprovePriceAdjustment;
using SmartService.Application.Features.PriceAdjustments.Commands.RejectPriceAdjustment;
using SmartService.Application.Features.PriceAdjustments.Commands.CreatePriceAdjustmentRequest;
using SmartService.Application.Features.PriceAdjustments.Queries.GetPendingPriceAdjustments;
using SmartService.Application.Features.PriceAdjustments.Queries.GetPriceAdjustmentByServiceRequest;
using SmartService.Domain.ValueObjects;
using System.Security.Claims;

namespace SmartService.API.Controllers;

[ApiController]
[Route("api/price-adjustments")]
public class PriceAdjustmentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IWebHostEnvironment _environment;

    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp",
        ".gif",
        ".heic",
        ".heif"
    };

    public PriceAdjustmentsController(IMediator mediator, IWebHostEnvironment environment)
    {
        _mediator = mediator;
        _environment = environment;
    }

    [Authorize(Roles = UserRoleConstants.Agent)]
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create(
        [FromForm] Guid serviceRequestId,
        [FromForm] decimal newPriceAmount,
        [FromForm] string newPriceCurrency,
        [FromForm] string reason,
        [FromForm] Guid createdBy,
        IFormFile evidenceImage,
        CancellationToken cancellationToken)
    {
        if (evidenceImage == null || evidenceImage.Length == 0)
            return BadRequest("Ảnh bằng chứng là bắt buộc.");

        var extension = System.IO.Path.GetExtension(evidenceImage.FileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedImageExtensions.Contains(extension))
        {
            extension = ".jpg";
        }

        var webRootPath = _environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRootPath))
        {
            webRootPath = System.IO.Path.Combine(_environment.ContentRootPath, "wwwroot");
        }

        var uploadDirectory = System.IO.Path.Combine(webRootPath, "uploads", "price-adjustments");
        System.IO.Directory.CreateDirectory(uploadDirectory);

        var storedFileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var filePath = System.IO.Path.Combine(uploadDirectory, storedFileName);

        await using (var fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.None))
        {
            await evidenceImage.CopyToAsync(fileStream, cancellationToken);
        }

        var imageUrl = $"/uploads/price-adjustments/{storedFileName}";

        var command = new CreatePriceAdjustmentRequestCommand
        {
            ServiceRequestId = serviceRequestId,
            NewPriceAmount = newPriceAmount,
            NewPriceCurrency = newPriceCurrency ?? "VND",
            Reason = reason,
            CreatedBy = createdBy,
            EvidenceImageUrl = imageUrl
        };

        var result = await _mediator.Send(command, cancellationToken);

        return Ok(result);
    }

    [Authorize(Roles = $"{UserRoleConstants.Staff},{UserRoleConstants.Admin}")]
    [HttpGet("pending")]
    public async Task<ActionResult<List<PriceAdjustmentDto>>> GetPending(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPendingPriceAdjustmentsQuery(), cancellationToken);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("/uploads/price-adjustments/{fileName}")]
    public IActionResult GetEvidenceImage([FromRoute] string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName) ||
            fileName.Contains("..", StringComparison.Ordinal) ||
            fileName.Contains('/') ||
            fileName.Contains('\\'))
        {
            return BadRequest("Tên tệp không hợp lệ.");
        }

        var webRootPath = _environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRootPath))
        {
            webRootPath = System.IO.Path.Combine(_environment.ContentRootPath, "wwwroot");
        }

        var filePath = System.IO.Path.Combine(webRootPath, "uploads", "price-adjustments", fileName);
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        var contentTypeProvider = new FileExtensionContentTypeProvider();
        if (!contentTypeProvider.TryGetContentType(filePath, out var contentType))
        {
            contentType = "application/octet-stream";
        }

        return PhysicalFile(filePath, contentType);
    }

    [Authorize(Roles = $"{UserRoleConstants.Agent},{UserRoleConstants.Staff},{UserRoleConstants.Admin}")]
    [HttpGet("service-request/{serviceRequestId}")]
    public async Task<ActionResult<PriceAdjustmentDto>> GetByServiceRequestId([FromRoute] Guid serviceRequestId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPriceAdjustmentByServiceRequestQuery(serviceRequestId), cancellationToken);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [Authorize(Roles = $"{UserRoleConstants.Staff},{UserRoleConstants.Admin}")]
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _mediator.Send(new ApprovePriceAdjustmentCommand(id, userId), cancellationToken);
        return Ok();
    }

    [Authorize(Roles = $"{UserRoleConstants.Staff},{UserRoleConstants.Admin}")]
    [HttpPost("{id}/reject")]
    public async Task<IActionResult> Reject([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _mediator.Send(new RejectPriceAdjustmentCommand(id, userId), cancellationToken);
        return Ok();
    }
}
