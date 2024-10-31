using BupaTest.Services;
using Serilog;

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddRazorPages();
    builder.Services.AddServerSideBlazor();
    builder.Services.AddSingleton<MotApiService>();
    builder.Services.AddHttpClient();
    builder.Services.AddHttpContextAccessor();

    var app = builder.Build();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();

    app.MapBlazorHub();
    app.MapFallbackToPage("/_Host");

    app.Run();
}
catch (Exception ex)
{
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .WriteTo.Console()
        .WriteTo.File("logs/myapp.txt", rollingInterval: RollingInterval.Day)
        .CreateLogger();
    Log.Error(ex, "An unhandled exception occurred.");
}
