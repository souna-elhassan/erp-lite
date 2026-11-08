using ErpLite.Application.Common;
using ErpLite.Application.Products;

namespace ErpLite.Api.Endpoints;

internal static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products").WithTags("Products");

        group.MapGet("/", async (string? search, ProductService service, CancellationToken ct) =>
                TypedResults.Ok(await service.ListAsync(search, ct)))
            .WithName("ListProducts");

        group.MapGet("/{id:guid}", async (Guid id, ProductService service, CancellationToken ct) =>
                ToHttp(await service.GetAsync(id, ct)))
            .WithName("GetProduct");

        group.MapPost("/", async (CreateProductRequest request, ProductService service, CancellationToken ct) =>
            {
                var result = await service.CreateAsync(request, ct);
                return result.IsSuccess
                    ? TypedResults.Created($"/api/products/{result.Value!.Id}", result.Value)
                    : Problem(result.Error!);
            })
            .WithName("CreateProduct");

        group.MapPut("/{id:guid}", async (Guid id, UpdateProductRequest request, ProductService service, CancellationToken ct) =>
                ToHttp(await service.UpdateAsync(id, request, ct)))
            .WithName("UpdateProduct");

        group.MapPost("/{id:guid}/stock/receive", async (Guid id, StockMovementRequest request, ProductService service, CancellationToken ct) =>
                ToHttp(await service.ReceiveStockAsync(id, request, ct)))
            .WithName("ReceiveStock");

        group.MapPost("/{id:guid}/stock/issue", async (Guid id, StockMovementRequest request, ProductService service, CancellationToken ct) =>
                ToHttp(await service.IssueStockAsync(id, request, ct)))
            .WithName("IssueStock");

        return app;
    }

    private static IResult ToHttp<T>(Result<T> result) =>
        result.IsSuccess ? TypedResults.Ok(result.Value) : Problem(result.Error!);

    private static IResult Problem(Error error) => TypedResults.Problem(
        title: error.Message,
        statusCode: error.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest,
        });
}
