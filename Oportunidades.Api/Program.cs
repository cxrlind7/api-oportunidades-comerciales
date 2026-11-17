using System.Text.Json.Serialization;
using Oportunidades.Api.Extensions;
using Oportunidades.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter())); // para que el estado salga como texto y no como número

builder.Services.ConfigureValidationResponses();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddJwtAuthentication();
builder.Services.AddOportunidades();
builder.Services.AddSwaggerWithJwt();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages(); // para que 401 y 403 también regresen ProblemDetails

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// lo necesitan las pruebas de integración (WebApplicationFactory<Program>)
public partial class Program { }
