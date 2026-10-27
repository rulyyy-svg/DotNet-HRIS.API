using HRIS.API.Common;
using Microsoft.Extensions.Time.Testing;

namespace HRIS.API.Tests.Common;

public class EmployeePolicyTests
{
    private static readonly DateTimeOffset FixedNow =
        new(2026, 10, 19, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void IsOnProbation_HiredTwoMonthsAgo_ReturnsTrue()
    {
        // Arrange
        var time = new FakeTimeProvider(FixedNow);
        var sut = new EmployeePolicy(time);

        // Act
        var result = sut.IsOnProbation(
            new DateOnly(2026, 8, 19));

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsOnProbation_HiredFourMonthsAgo_ReturnsFalse()
    {
        // Arrange
        var time = new FakeTimeProvider(FixedNow);
        var sut = new EmployeePolicy(time);

        // Act
        var result = sut.IsOnProbation(
            new DateOnly(2026, 6, 19));

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData(90, true)]
    [InlineData(91, false)]
    public void IsValidHireDate_ReturnsExpected(
        int daysAhead,
        bool expected)
    {
        // Arrange
        var time = new FakeTimeProvider(FixedNow);
        var sut = new EmployeePolicy(time);
        var hireDate = sut.Today.AddDays(daysAhead);

        // Act
        var result = sut.IsValidHireDate(hireDate);

        // Assert
        Assert.Equal(expected, result);
    }
}