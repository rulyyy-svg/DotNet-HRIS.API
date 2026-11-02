
using HRIS.API.Common.Exceptions;
using HRIS.API.Data.Repositories;
using HRIS.API.DTOs.Departments;
using HRIS.API.Entities;
using HRIS.API.Features.Departments.Commands.CreateDepartment;
using Moq;
using Xunit;

namespace HRIS.API.Tests;

public class CreateDepartmentCommandHandlerTests
{
    private readonly Mock<IDepartmentRepository> _departments = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly CreateDepartmentCommandHandler _sut;

    public CreateDepartmentCommandHandlerTests()
    {
        _uow.Setup(u => u.Departments)
            .Returns(_departments.Object);

        _sut = new CreateDepartmentCommandHandler(_uow.Object);
    }

    [Fact]
    public async Task Handle_DuplicateCode_ThrowsDuplicateDepartmentException_AndSavesNothing()
    {
        // Arrange
        _departments
            .Setup(r => r.CodeExistsAsync(
                "TECH",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new CreateDepartmentCommand(
            new CreateDepartmentRequest(
                " tech ",
                "Technology Copy",
                null));

        // Act
        var ex = await Assert.ThrowsAsync<DuplicateDepartmentException>(
            () => _sut.Handle(
                command,
                CancellationToken.None));

        // Assert
        Assert.Equal(409, ex.StatusCode);
        Assert.Contains("TECH", ex.Message);

        _departments.Verify(
            r => r.AddAsync(
                It.IsAny<Department>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _uow.Verify(
            u => u.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_NewCode_StoresTheCodeTrimmedAndUpperCased()
    {
        // Arrange
        var command = new CreateDepartmentCommand(
            new CreateDepartmentRequest(
                " qa ",
                "Quality Assurance",
                null));

        // Act
        var id = await _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        _departments.Verify(
            r => r.AddAsync(
                It.Is<Department>(d =>
                    d.Id == id &&
                    d.Code == "QA" &&
                    d.Name == "Quality Assurance"),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _uow.Verify(
            u => u.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
