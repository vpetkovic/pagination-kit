namespace PaginationKit.AspNetCore.Validation;

public class CursorPaginationRequestValidationModel
{
    public string? Cursor { get; set; }
    public int? Limit { get; set; }
}
