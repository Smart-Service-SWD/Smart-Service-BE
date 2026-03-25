using MediatR;
using SmartService.Application.Common.Models.Payments;

namespace SmartService.Application.Features.Payments.Commands.HandlePayOSWebhook;

public record HandlePayOSWebhookCommand(WebhookData Data) : IRequest<Unit>;
