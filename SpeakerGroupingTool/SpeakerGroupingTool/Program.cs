using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.EntityFrameworkCore;
using Microsoft.FluentUI.AspNetCore.Components;
using SpeakerGroupingTool;
using SpeakerGroupingTool.Client.Pages;
using SpeakerGroupingTool.Components;
using SpeakerGroupingTool.Components.Account;
using SpeakerGroupingTool.Data;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();
builder.Services.AddFluentUIComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, PersistingRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddSingleton<WebsocketService>();
builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();



app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(SpeakerGroupingTool.Client._Imports).Assembly);
app.MapAdditionalIdentityEndpoints();

app.UseWebSockets();

app.Use(async (context, next) =>
{
    if (context.Request.Path == "/ws")
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            await Echo(webSocket, app.Services.GetRequiredService<WebsocketService>());
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }
    else
    {
        await next(context);
    }

});

async Task Echo(WebSocket webSocket, WebsocketService websocketService)
{
    //var buffer = new byte[1024 * 1024];
    //var aa = await webSocket.ReceiveAsync(buffer, CancellationToken.None);

    //var name = Encoding.UTF8.GetString(buffer, 0, aa.Count);

    var name =  await websocketService.AddWebSocket(webSocket);


    while (webSocket.State == WebSocketState.Open)
    {
        await Task.Delay(10000);

    }
    await websocketService.RemoveWebSocket(name);

    //while (webSocket.State == WebSocketState.Open)
    //{
    //    aa = await webSocket.ReceiveAsync(buffer, CancellationToken.None);

    //    var data = Encoding.UTF8.GetString(buffer, 0, aa.Count);
    //    try
    //    {
    //        var json = JsonDocument.Parse(data).RootElement;
    //        var type = json.GetProperty("type").GetString();

    //        if (type == )

    //    }
    //    catch (Exception ex )
    //    {

    //        Console.WriteLine(ex.Message);
    //    }

    //}
    //for (int i = 0; i < 100; ++i)
    //{
    //    await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("32342")), WebSocketMessageType.Text, false, CancellationToken.None);
    //    await Task.Delay(500);
    //    Console.WriteLine(Encoding.UTF8.GetString(buffer, 0, aa.Count));
    //    await webSocket.SendAsync(Encoding.UTF8.GetBytes("sada"), WebSocketMessageType.Text, false, CancellationToken.None);
    //}
    //webSocket.ReceiveAsync
}

// Add additional endpoints required by the Identity /Account Razor components.

app.Run();

