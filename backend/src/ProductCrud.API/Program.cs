using ProductCrud.API.GraphQL.Errors;
using ProductCrud.API.GraphQL.Mutations;
using ProductCrud.API.GraphQL.Queries;
using ProductCrud.API.Middleware;
using ProductCrud.Application;
using ProductCrud.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// CORS for the Next.js frontend
const string FrontendCors = "Frontend";
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCors, policy => policy
        .WithOrigins("http://localhost:3000")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .WithExposedHeaders(CorrelationIdMiddleware.HeaderName)); // let the browser read the ID
});

// GraphQL (Hot Chocolate)
builder.Services
    .AddGraphQLServer()
    .AddQueryType<ProductQueries>()
    .AddMutationType<ProductMutations>()
    .AddErrorFilter<GraphQLErrorFilter>()
    .ModifyRequestOptions(o => o.IncludeExceptionDetails = builder.Environment.IsDevelopment());

var app = builder.Build();

// Middleware pipeline (order matters)
app.UseApiMiddleware();      // CorrelationId -> RequestLogging -> ExceptionHandling
app.UseCors(FrontendCors);

app.MapGraphQL(); // http://localhost:5000/graphql (Nitro IDE in the browser)

app.Run();
