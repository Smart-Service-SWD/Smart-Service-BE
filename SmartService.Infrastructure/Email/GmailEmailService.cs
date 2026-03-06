using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using SmartService.Application.Abstractions.Notifications;

namespace SmartService.Infrastructure.Email;

/// <summary>
/// Gmail SMTP email service using MailKit.
/// Uses Gmail App Password (not the main account password).
/// </summary>
public class GmailEmailService : IEmailService
{
    private readonly EmailSettings _settings;

    public GmailEmailService(IOptions<EmailSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task SendWelcomeEmailAsync(
        string toEmail,
        string fullName,
        string temporaryPassword,
        CancellationToken cancellationToken = default)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
        message.To.Add(new MailboxAddress(fullName, toEmail));
        message.Subject = "🎉 Chào mừng bạn đến với Smart Service - Thông tin đăng nhập";

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = BuildWelcomeEmailHtml(fullName, toEmail, temporaryPassword),
            TextBody = BuildWelcomeEmailText(fullName, toEmail, temporaryPassword)
        };
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, SecureSocketOptions.StartTls, cancellationToken);
        await client.AuthenticateAsync(_settings.SenderEmail, _settings.AppPassword, cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }

    public async Task SendPasswordResetEmailAsync(
        string toEmail,
        string fullName,
        string otp,
        CancellationToken cancellationToken = default)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
        message.To.Add(new MailboxAddress(fullName, toEmail));
        message.Subject = "🔒 Smart Service - Mã xác nhận đặt lại mật khẩu";

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = BuildPasswordResetEmailHtml(fullName, otp),
            TextBody = BuildPasswordResetEmailText(fullName, otp)
        };
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, SecureSocketOptions.StartTls, cancellationToken);
        await client.AuthenticateAsync(_settings.SenderEmail, _settings.AppPassword, cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }

    private static string BuildPasswordResetEmailHtml(string fullName, string otp)
    {
        return $"""
        <!DOCTYPE html>
        <html lang="vi">
        <head><meta charset="UTF-8"><meta name="viewport" content="width=device-width, initial-scale=1.0"></head>
        <body style="font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 20px;">
          <div style="max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.1);">

            <!-- Header -->
            <div style="background: linear-gradient(135deg, #CC3300, #FF6633); padding: 40px 30px; text-align: center;">
              <h1 style="color: #ffffff; margin: 0; font-size: 28px;">🛠️ Smart Service</h1>
              <p style="color: rgba(255,255,255,0.9); margin: 8px 0 0;">Yêu cầu đặt lại mật khẩu</p>
            </div>

            <!-- Body -->
            <div style="padding: 40px 30px;">
              <h2 style="color: #333; margin-top: 0;">Xin chào, {fullName}! 👋</h2>
              <p style="color: #555; line-height: 1.6;">
                Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.
                Sử dụng mã OTP dưới đây để đặt lại mật khẩu:
              </p>

              <!-- OTP Box -->
              <div style="text-align: center; margin: 32px 0;">
                <div style="display: inline-block; background: linear-gradient(135deg, #CC3300, #FF6633); border-radius: 12px; padding: 20px 40px;">
                  <p style="color: rgba(255,255,255,0.8); font-size: 14px; margin: 0 0 8px; letter-spacing: 1px;">MÃ XÁC NHẬN</p>
                  <span style="color: #ffffff; font-size: 42px; font-weight: bold; letter-spacing: 10px;">{otp}</span>
                </div>
              </div>

              <!-- Warning -->
              <div style="background-color: #fff8e1; border-left: 4px solid #FFA000; padding: 16px; border-radius: 0 8px 8px 0; margin: 20px 0;">
                <p style="margin: 0; color: #7B5800; font-size: 14px;">
                  ⚠️ <strong>Lưu ý bảo mật:</strong> Mã OTP này có hiệu lực trong <strong>15 phút</strong> kể từ khi gửi.
                  Không chia sẻ mã này với bất kỳ ai.
                </p>
              </div>

              <div style="background-color: #f0f4ff; border-left: 4px solid #3366CC; padding: 16px; border-radius: 0 8px 8px 0;">
                <p style="margin: 0; color: #1a337a; font-size: 14px;">
                  🛑 Nếu bạn không thực hiện yêu cầu này, hãy bỏ qua email này. Tài khoản của bạn vẫn an toàn.
                </p>
              </div>
            </div>

            <!-- Footer -->
            <div style="background-color: #f8f8f8; padding: 20px 30px; text-align: center; border-top: 1px solid #e0e0e0;">
              <p style="color: #999; font-size: 12px; margin: 0;">
                Email này được gửi tự động từ hệ thống Smart Service. Vui lòng không trả lời email này.
              </p>
            </div>
          </div>
        </body>
        </html>
        """;
    }

    private static string BuildPasswordResetEmailText(string fullName, string otp)
    {
        return $"""
        Xin chào, {fullName}!

        Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.

        Mã OTP của bạn: {otp}
        Hiệu lực: 15 phút

        Nếu bạn không thực hiện yêu cầu này, hãy bỏ qua email này.

        Smart Service Team
        """;
    }

    private static string BuildWelcomeEmailHtml(string fullName, string email, string password)
    {
        return $"""
        <!DOCTYPE html>
        <html lang="vi">
        <head><meta charset="UTF-8"><meta name="viewport" content="width=device-width, initial-scale=1.0"></head>
        <body style="font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 20px;">
          <div style="max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.1);">
            
            <!-- Header -->
            <div style="background: linear-gradient(135deg, #0066CC, #0099FF); padding: 40px 30px; text-align: center;">
              <h1 style="color: #ffffff; margin: 0; font-size: 28px;">🛠️ Smart Service</h1>
              <p style="color: rgba(255,255,255,0.9); margin: 8px 0 0;">Hệ thống quản lý dịch vụ thông minh</p>
            </div>

            <!-- Body -->
            <div style="padding: 40px 30px;">
              <h2 style="color: #333; margin-top: 0;">Chào mừng, {fullName}! 👋</h2>
              <p style="color: #555; line-height: 1.6;">
                Tài khoản của bạn đã được tạo thành công trên hệ thống Smart Service.
                Dưới đây là thông tin đăng nhập của bạn:
              </p>

              <!-- Credentials Box -->
              <div style="background-color: #f0f7ff; border: 2px solid #0066CC; border-radius: 10px; padding: 24px; margin: 24px 0;">
                <table style="width: 100%; border-collapse: collapse;">
                  <tr>
                    <td style="padding: 8px 0; color: #666; font-size: 14px; width: 140px;">📧 Email đăng nhập:</td>
                    <td style="padding: 8px 0; color: #0066CC; font-weight: bold; font-size: 14px;">{email}</td>
                  </tr>
                  <tr>
                    <td style="padding: 8px 0; color: #666; font-size: 14px;">🔑 Mật khẩu tạm thời:</td>
                    <td style="padding: 8px 0;">
                      <span style="background-color: #0066CC; color: #ffffff; padding: 6px 14px; border-radius: 6px; font-size: 16px; font-weight: bold; letter-spacing: 1px;">{password}</span>
                    </td>
                  </tr>
                </table>
              </div>

              <!-- Warning -->
              <div style="background-color: #fff8e1; border-left: 4px solid #FFA000; padding: 16px; border-radius: 0 8px 8px 0; margin: 20px 0;">
                <p style="margin: 0; color: #7B5800; font-size: 14px;">
                  ⚠️ <strong>Lưu ý bảo mật:</strong> Đây là mật khẩu tạm thời được tạo tự động.
                  Vui lòng đăng nhập và đổi mật khẩu ngay sau khi nhận được email này.
                </p>
              </div>

              <p style="color: #555; font-size: 14px; line-height: 1.6;">
                Nếu bạn có bất kỳ câu hỏi nào, vui lòng liên hệ quản trị viên hệ thống.
              </p>
            </div>

            <!-- Footer -->
            <div style="background-color: #f8f8f8; padding: 20px 30px; text-align: center; border-top: 1px solid #e0e0e0;">
              <p style="color: #999; font-size: 12px; margin: 0;">
                Email này được gửi tự động từ hệ thống Smart Service. Vui lòng không trả lời email này.
              </p>
            </div>
          </div>
        </body>
        </html>
        """;
    }

    private static string BuildWelcomeEmailText(string fullName, string email, string password)
    {
        return $"""
        Chào mừng, {fullName}!

        Tài khoản của bạn đã được tạo thành công trên hệ thống Smart Service.

        Thông tin đăng nhập:
        - Email: {email}
        - Mật khẩu tạm thời: {password}

        Lưu ý: Đây là mật khẩu tạm thời, vui lòng đổi mật khẩu sau khi đăng nhập.

        Smart Service Team
        """;
    }
}
