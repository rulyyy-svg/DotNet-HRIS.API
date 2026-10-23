namespace HRIS.API.DTOs.Positions;

public record CreatePositionRequest(
    string Title,
    int Level);