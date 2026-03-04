namespace SmartService.Infrastructure.Email;

/// <summary>
/// Configuration settings for Gmail SMTP.
/// Maps from appsettings.json → "EmailSettings" section.
/// </summary>
public class EmailSettings
{
    public string SenderEmail { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    /// <summary>Gmail App Password (16 chars, no spaces)</summary>
    public string AppPassword { get; set; } = string.Empty;
    public string SmtpHost { get; set; } = "smtp.gmail.com";
    public int SmtpPort { get; set; } = 587;
}
