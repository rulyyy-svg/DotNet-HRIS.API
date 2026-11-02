
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using HRIS.API.Services.Email;
using Microsoft.AspNetCore.Mvc;

namespace HRIS.API.Controllers;

/// <summary>
/// Module 9 helper: sends one test email.
/// Development only.
/// </summary>
[ApiController]
[Route("api/dev")]
public class DevController(
    IEmailService emailService,
    IWebHostEnvironment env
) : ControllerBase
{
    public record TestEmailRequest(
        [Required, EmailAddress] string To,
        [Required] string FullName
    );

    [HttpPost("test-email")]
    public async Task<IActionResult> TestEmail(
        TestEmailRequest request,
        CancellationToken ct)
    {
        // 1. Hanya berjalan di Development
        if (!env.IsDevelopment())
            return NotFound();

        // 2. Encode input agar aman ketika dimasukkan ke HTML
        var name = HtmlEncoder.Default.Encode(request.FullName);

        try
        {
            // 3. Mengirim email menggunakan IEmailService
            await emailService.SendEmailAsync(
                new MailRequest
                {
                    ToEmail = request.To,

                    Subject = $"HRIS test email for {request.FullName}",

                    HtmlBody = $"""
                        <h1>Hello {name}</h1>
                        <p>This is a test email from <strong>HRIS</strong>.</p>
                        """,

                    TextBody = $"Hello {request.FullName}, this is a test email from HRIS."
                },
                ct
            );
        }
        catch (EmailDeliveryException ex)
        {
            // 4. Menampilkan error khusus Development
            return Problem(
                statusCode: StatusCodes.Status502BadGateway,
                title: "The email could not be delivered.",
                detail: ex.Message
            );
        }

        // 5. Email berhasil dikirim
        return NoContent();
    }
}
