using rinha_backend;
using rinha_backend.endpoints;

var builder = WebApplication.CreateSlimBuilder(args);
lazy.connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// Código de depuração para verificar a string de conexão
Console.WriteLine($"Connection String: {lazy.connectionString}");
var app = builder.Build();
app.MapClienteEndpoint();
app.Run();
