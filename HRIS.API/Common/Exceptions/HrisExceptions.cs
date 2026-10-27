namespace HRIS.API.Common.Exceptions;

public abstract class HrisException : Exception
{
    protected HrisException() { }

    protected HrisException(string message) : base(message) { }

    protected HrisException(string message, Exception innerException)
        : base(message, innerException) { }

    public abstract int StatusCode { get; }

    public abstract string Title { get; }

    public virtual string ErrorCode => "HRIS_ERROR";
}

// 401
public class UnauthorizedException : HrisException
{
    public UnauthorizedException()
        : base("Authentication is required.") { }

    public UnauthorizedException(string message) : base(message) { }

    public UnauthorizedException(string message, Exception innerException)
        : base(message, innerException) { }

    public override int StatusCode => StatusCodes.Status401Unauthorized;
    public override string Title => "Unauthorized";
    public override string ErrorCode => "UNAUTHENTICATED";
}

// 403
public class ForbiddenAccessException : HrisException
{
    public ForbiddenAccessException()
        : base("You do not have access to this resource.") { }

    public ForbiddenAccessException(string message) : base(message) { }

    public ForbiddenAccessException(string message, Exception innerException)
        : base(message, innerException) { }

    public override int StatusCode => StatusCodes.Status403Forbidden;
    public override string Title => "Forbidden";
    public override string ErrorCode => "FORBIDDEN";
}

// 404
public class NotFoundException : HrisException
{
    public NotFoundException()
        : base("The requested resource was not found.") { }

    public NotFoundException(string message) : base(message) { }

    public NotFoundException(string message, Exception innerException)
        : base(message, innerException) { }

    public NotFoundException(string resource, object key)
        : base($"{resource} with id '{key}' was not found.")
    {
        Resource = resource;
        Key = key;
    }

    public string? Resource { get; }
    public object? Key { get; }

    public override int StatusCode => StatusCodes.Status404NotFound;
    public override string Title => "Resource not found";
    public override string ErrorCode => "NOT_FOUND";
}

// 409
public class ConflictException : HrisException
{
    public ConflictException()
        : base("The request conflicts with the current state of the resource.") { }

    public ConflictException(string message) : base(message) { }

    public ConflictException(string message, Exception innerException)
        : base(message, innerException) { }

    public override int StatusCode => StatusCodes.Status409Conflict;
    public override string Title => "Conflict";
    public override string ErrorCode => "CONFLICT";
}

// 422
public class BusinessRuleException : HrisException
{
    public BusinessRuleException()
        : base("A business rule was violated.") { }

    public BusinessRuleException(string message) : base(message) { }

    public BusinessRuleException(string message, Exception innerException)
        : base(message, innerException) { }

    public override int StatusCode => StatusCodes.Status422UnprocessableEntity;
    public override string Title => "Business rule violated";
    public override string ErrorCode => "BUSINESS_RULE_VIOLATION";
}

// Specific exceptions

public class DuplicateEmployeeException : ConflictException
{
    public DuplicateEmployeeException(string message) : base(message) { }

    public DuplicateEmployeeException(
        string message,
        Exception innerException) : base(message, innerException) { }

    public override string ErrorCode => "EMPLOYEE_DUPLICATE";
}

public class DuplicateDepartmentException : ConflictException
{
    public DuplicateDepartmentException(string message) : base(message) { }

    public DuplicateDepartmentException(
        string message,
        Exception innerException) : base(message, innerException) { }

    public override string ErrorCode => "DEPARTMENT_DUPLICATE";
}

public class EmployeeAlreadyTerminatedException : ConflictException
{
    public EmployeeAlreadyTerminatedException(Guid employeeId)
        : base($"Employee '{employeeId}' is already terminated.")
    {
        EmployeeId = employeeId;
    }

    public Guid EmployeeId { get; }

    public override string ErrorCode => "EMPLOYEE_ALREADY_TERMINATED";
}

public class InvalidReportingLineException : BusinessRuleException
{
    public InvalidReportingLineException(string message) : base(message) { }

    public InvalidReportingLineException(
        string message,
        Exception innerException) : base(message, innerException) { }

    public override string ErrorCode => "INVALID_REPORTING_LINE";
}