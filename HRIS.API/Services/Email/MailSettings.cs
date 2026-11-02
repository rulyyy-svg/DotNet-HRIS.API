
using MailKit.Security;

namespace HRIS.API.Services.Email;

public class MailSettings
{
    public const string SectionName = "MailSettings";

    public string Host { get; set; } = "localhost";

    public int Port { get; set; } = 2525;

    public SecureSocketOptions Security { get; set; }
        = SecureSocketOptions.Auto;

    public string SenderName { get; set; } = "HRIS";

    public string SenderEmail { get; set; }
        = "no-reply@hris.local";

    public string? UserName { get; set; }

    public string? Password { get; set; }
}
