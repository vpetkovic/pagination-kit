namespace WebApi.Models;

public record Product(int Id, string Name, string Category, decimal Price, DateTime CreatedDate);
