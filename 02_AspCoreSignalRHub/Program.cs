using Microsoft.AspNetCore.HttpLogging;
using SignalRServer1.Hubs;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.All;
    logging.RequestHeaders.Add("sec-ch-ua");
    logging.ResponseHeaders.Add("MyResponseHeader");
    logging.MediaTypeOptions.AddText("application/javascript");
    logging.RequestBodyLogLimit = 4096;
    logging.ResponseBodyLogLimit = 4096;
    logging.CombineLogs = true;
});


var app = builder.Build();
app.UseHttpLogging();
//app.MapGet("/", () => "Hello World!");
app.UseStaticFiles();
app.MapFallbackToFile("index.html");
app.MapHub<ChatHub>("/chat");

app.Run();
