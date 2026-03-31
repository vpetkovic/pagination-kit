namespace PaginationKit;

/// <summary>
/// Direction of cursor-based traversal.
/// </summary>
public enum CursorDirection
{
    /// <summary>
    /// Retrieve items after the cursor (moving forward through results).
    /// </summary>
    Forward,

    /// <summary>
    /// Retrieve items before the cursor (moving backward through results).
    /// </summary>
    Backward
}
