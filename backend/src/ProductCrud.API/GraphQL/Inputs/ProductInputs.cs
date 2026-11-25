namespace ProductCrud.API.GraphQL.Inputs;

// GraphQL input types. Names match the frontend mutations:
//   mutation CreateProduct($input: CreateProductInput!)
//   mutation UpdateProduct($input: UpdateProductInput!)

public record CreateProductInput(string Name, string? Description, decimal Price, int Quantity);

public record UpdateProductInput(int Id, string Name, string? Description, decimal Price, int Quantity);
