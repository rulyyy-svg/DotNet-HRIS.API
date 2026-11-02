
namespace HRIS.API.Services.Email;

public interface IEmailService
{
    Task SendEmailAsync(
        MailRequest request,
        CancellationToken ct = default
    );
}
