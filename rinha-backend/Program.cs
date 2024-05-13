using rinha_backend.endpoints;

var builder = WebApplication.CreateSlimBuilder(args);
var app = builder.Build();
app.MapClienteEndpoint();
app.Run();
