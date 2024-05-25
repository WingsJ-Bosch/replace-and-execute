using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using replace_and_execute;


var configurationBuilder = new ConfigurationBuilder();
configurationBuilder.SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
var configuration = (configurationBuilder).Build();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Replace and Execute", Version = "v1" });
    options.OperationFilter<UploadFileOperationFilter>();
});
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(configuration.GetValue<int>("port", 3000));
    options.Limits.MaxRequestBodySize = 100 * 1024 * 1024; // 100MB
});

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();

