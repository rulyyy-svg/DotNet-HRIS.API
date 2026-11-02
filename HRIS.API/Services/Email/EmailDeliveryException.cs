
namespace HRIS.API.Services.Email;

/// <summary>
/// Exception yang digunakan ketika email gagal dikirim.
/// </summary>
public class EmailDeliveryException : Exception
{
    public EmailDeliveryException()
        : base("The email could not be delivered.")
    {
    }

    public EmailDeliveryException(string message)
        : base(message)
    {
    }

    public EmailDeliveryException(
        string message,
        Exception innerException)
        : base(message, innerException)
    {
    }
}
