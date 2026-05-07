var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/hello", () => Results.Ok(new { message = "Bienvenido a Nido!" }));

app.Run();
