
using System.IO;
using System.Net.Sockets;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace HRIS.API.Services.Email;

public class MailKitEmailService(
    IOptions<MailSettings> options,
    ILogger<MailKitEmailService> logger
) : IEmailService
{
    private readonly MailSettings _settings = options.Value;

    public async Task SendEmailAsync(
        MailRequest request,
        CancellationToken ct = default)
    {
        // 1. Membuat email message
        var message = new MimeMessage();

        // 2. Mengatur pengirim
        message.From.Add(
            new MailboxAddress(
                _settings.SenderName,
                _settings.SenderEmail
            )
        );

        // 3. Mengatur penerima
        message.To.Add(
            MailboxAddress.Parse(request.ToEmail)
        );

        // 4. Mengatur subject
        message.Subject = request.Subject;

        // 5. Membuat isi email HTML dan Text
        message.Body = new BodyBuilder
        {
            HtmlBody = request.HtmlBody,
            TextBody = request.TextBody
                ?? "Please open this email in an HTML-capable email client."
        }.ToMessageBody();

        // 6. Membuat SMTP client
        using var smtp = new SmtpClient();

        try
        {
            // 7. Menghubungkan ke SMTP server
            await smtp.ConnectAsync(
                _settings.Host,
                _settings.Port,
                _settings.Security,
                ct
            );

            // 8. Melakukan autentikasi jika diperlukan
            if (!string.IsNullOrEmpty(_settings.UserName))
            {
                await smtp.AuthenticateAsync(
                    _settings.UserName,
                    _settings.Password ?? "",
                    ct
                );
            }

            // 9. Mengirim email
            await smtp.SendAsync(message, ct);

            // 10. Memutuskan koneksi SMTP
            await smtp.DisconnectAsync(true, ct);
        }
        catch (AuthenticationException ex)
        {
            throw new EmailDeliveryException(
                $"SMTP authentication failed for host '{_settings.Host}'. Check MailSettings.",
                ex
            );
        }
        catch (NotSupportedException ex)
        {
            throw new EmailDeliveryException(
                $"The SMTP server {_settings.Host}:{_settings.Port} does not support the requested security or login mode.",
                ex
            );
        }
        catch (SmtpCommandException ex)
        {
            throw new EmailDeliveryException(
                $"The SMTP server rejected the message (error {ex.ErrorCode}, status {ex.StatusCode}).",
                ex
            );
        }
        catch (Exception ex) when (
            ex is SmtpProtocolException
            or SslHandshakeException
            or SocketException
            or IOException)
        {
            throw new EmailDeliveryException(
                $"The SMTP server {_settings.Host}:{_settings.Port} could not be reached.",
                ex
            );
        }

        // 11. Logging ketika email berhasil dikirim
        logger.LogDebug(
            "Email with subject {Subject} sent through {SmtpHost}",
            request.Subject,
            _settings.Host
        );
    }
}
