using rinha_backend;
using rinha_backend.endpoints;

var builder = WebApplication.CreateSlimBuilder(args);
builder.Services.AddMemoryCache();
lazy.connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var app = builder.Build();
app.MapClienteEndpoint();
app.Run();
