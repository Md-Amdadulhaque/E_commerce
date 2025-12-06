using MCP_Server.Services;
using MCP_Server.Tools;
using MyMcpServer.Services;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient<SourceClient>(
    client =>
    {
        client.BaseAddress = new Uri(
        builder.Configuration["SourceProject:BaseUrl"]
        ?? "http://localhost:5000");
    }
    );
builder.Services.AddHttpClient<LLMClient>();

builder.Services.AddScoped<ToolExecutor>();
builder.Services.AddSingleton<IToolService, ToolService>();
builder.Services.AddSingleton<SourceProjectTools>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
