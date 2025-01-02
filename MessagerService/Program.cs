using Azure.Messaging.ServiceBus;
using MessagerService.Services;

var builder = WebApplication.CreateBuilder(args);

// Explicitly configure Kestrel to listen on the assigned port
var port = builder.Configuration["SERVICE_PORT"] ?? "5006";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Add EmailService
builder.Services.AddSingleton(provider =>
{
    var config = builder.Configuration.GetSection("AzureCommunicationServices");
    var connectionString = config.GetValue<string>("ConnectionString");
    var senderEmail = config.GetValue<string>("SenderEmail");
    return new EmailService(connectionString, senderEmail);
});

// Add ServiceBusClient and Listener
builder.Services.AddSingleton<ServiceBusClient>(provider =>
{
    var connectionString = provider.GetRequiredService<IConfiguration>()["ConnectionStrings:AzureServiceBus"];
    return new ServiceBusClient(connectionString);
});
builder.Services.AddSingleton<ServiceBusListener>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Start the ServiceBusListener
var listener = app.Services.GetRequiredService<ServiceBusListener>();
await listener.StartAsync();

app.Run();
