var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/date-time", () =>
{
    var currentTime = DateTime.Now;
    var data = currentTime.ToString("yyyy-MM-dd HH:mm:ss");
    return data;
});

app.Run();
