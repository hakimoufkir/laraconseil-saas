var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton(provider =>
{
    var config = builder.Configuration.GetSection("AzureCommunicationServices");
    var connectionString = config.GetValue<string>("ConnectionString");
    var senderEmail = config.GetValue<string>("SenderEmail");
    return new MessagerService.Services.EmailService(connectionString, senderEmail);
});

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
app.Run();
