
using HRIS.API.Common.Exceptions;
using HRIS.API.Data.Repositories;
using HRIS.API.Entities;
using HRIS.API.Features.Employees.Commands.CreateEmployee;
using HRIS.API.Models;
using HRIS.API.Services.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Xunit;

namespace HRIS.API.Tests;

public class CreateEmployeeCommandHandlerTests
{
    private readonly FakeTimeProvider _time = new(
        new DateTimeOffset(
            2026, 10, 19, 9, 0, 0, TimeSpan.Zero));

    private readonly Mock<IEmployeeRepository> _employees = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IEmailService> _emailService = new();
    private readonly Mock<IEmailTemplateRenderer> _renderer = new();

    private readonly IConfiguration _configuration;

    private readonly CreateEmployeeCommandHandler _sut;

    private static readonly CreateEmployeeCommand Command = new(
        new CreateEmployeeRequest(
            "EMP-0100",
            "Siti Aminah",
            " Siti.Aminah@Company.ID ",
            new DateOnly(2026, 11, 2),
            Guid.NewGuid(),
            Guid.NewGuid(),
            null));

    public CreateEmployeeCommandHandlerTests()
    {
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["App:FrontendUrl"] = "https://localhost:5173"
                })
            .Build();

        _uow
            .Setup(u => u.Employees)
            .Returns(_employees.Object);

        _renderer
            .Setup(r => r.RenderAsync(
                It.IsAny<string>(),
                It.IsAny<IReadOnlyDictionary<string, string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("<html>Welcome</html>");

        _emailService
            .Setup(s => s.SendEmailAsync(
                It.IsAny<MailRequest>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _sut = new CreateEmployeeCommandHandler(
            _uow.Object,
            _time,
            _emailService.Object,
            _renderer.Object,
            _configuration,
            NullLogger<CreateEmployeeCommandHandler>.Instance);
    }

    [Fact]
    public async Task Handle_NewEmployee_AddsEmployeeAndSavesOnce()
    {
        // Act
        var id = await _sut.Handle(
            Command,
            CancellationToken.None);

        // Assert
        _employees.Verify(
            r => r.AddAsync(
                It.Is<Employee>(e =>
                    e.Id == id &&
                    e.Email == "siti.aminah@company.id" &&
                    e.EmployeeNumber == "EMP-0100" &&
                    e.EmployeeStatus == EmployeeStatus.Active &&
                    e.CreatedAt == _time.GetUtcNow()),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _uow.Verify(
            u => u.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);

        _renderer.Verify(
            r => r.RenderAsync(
                "WelcomeEmployee",
                It.IsAny<IReadOnlyDictionary<string, string>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _emailService.Verify(
            s => s.SendEmailAsync(
                It.Is<MailRequest>(m =>
                    m.ToEmail == "siti.aminah@company.id" &&
                    m.Subject == "Welcome to the team, Siti Aminah"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_EmailDeliveryFails_StillReturnsEmployeeId()
    {
        // Arrange
        _emailService
            .Setup(s => s.SendEmailAsync(
                It.IsAny<MailRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(
                new EmailDeliveryException("SMTP unavailable"));

        // Act
        var id = await _sut.Handle(
            Command,
            CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, id);

        _uow.Verify(
            u => u.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateEmployeeNumber_ThrowsDuplicateEmployeeException_AndSavesNothing()
    {
        // Arrange
        _employees
            .Setup(r => r.EmployeeNumberExistsAsync(
                "EMP-0100",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var ex = await Assert.ThrowsAsync<DuplicateEmployeeException>(
            () => _sut.Handle(
                Command,
                CancellationToken.None));

        // Assert
        Assert.Contains("EMP-0100", ex.Message);

        _employees.Verify(
            r => r.AddAsync(
                It.IsAny<Employee>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _uow.Verify(
            u => u.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);

        _emailService.Verify(
            s => s.SendEmailAsync(
                It.IsAny<MailRequest>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
