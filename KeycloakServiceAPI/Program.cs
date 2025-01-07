using Azure.Messaging.ServiceBus;
using KeycloakServiceAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Explicitly configure Kestrel to listen on the assigned port
var port = builder.Configuration["SERVICE_PORT"] ?? "5007";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Register Services
builder.Services.AddHttpClient<KeycloakService>();
builder.Services.AddSingleton<ServiceBusClient>(provider =>
{
    var connectionString = builder.Configuration["ConnectionStrings:AzureServiceBus"];
    return new ServiceBusClient(connectionString);
});
builder.Services.AddSingleton<ServiceBusListener>();
builder.Services.AddSingleton<ServiceBusPublisher>();


// Add controllers
builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

// Start Service Bus Listener
var listener = app.Services.GetRequiredService<ServiceBusListener>();
await listener.StartAsync();

app.Run();
