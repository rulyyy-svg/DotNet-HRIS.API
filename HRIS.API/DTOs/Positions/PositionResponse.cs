namespace HRIS.API.DTOs.Positions;

public record PositionResponse(
    Guid Id,
    string Title,
    int Level);