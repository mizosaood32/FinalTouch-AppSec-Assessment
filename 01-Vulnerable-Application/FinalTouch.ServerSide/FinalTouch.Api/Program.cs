using FinalTouch.Api.Middleware;
using FinalTouch.Api.SignalR;
using FinalTouch.Core.Entities;
using Microsoft.AspNetCore.Identity;
using StackExchange.Redis;
using FinalTouch.InfraStructure.Data;
using FinalTouch.Application;

var builder = WebApplication.CreateBuilder(args);

// App Services (Infrastructure Layer)
builder.Services.AddInfrastructureServices(builder.Configuration);

// Redis separately
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var connString = builder.Configuration.GetConnectionString("Redis")
        ?? throw new Exception("Redis connection string not found.");
    var configuration = ConfigurationOptions.Parse(connString, true);
    return ConnectionMultiplexer.Connect(configuration);
});

// Identity
builder.Services.AddIdentityApiEndpoints<AppUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

// Controllers, Swagger, SignalR, etc.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddApplicationServices();

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAngularApp");

// Seed DeliveryMethods if table is empty
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (!context.DeliveryMethods.Any())
    {
        context.DeliveryMethods.AddRange(
            new DeliveryMethod { ShortName = "Standard", DeliveryTime = "5-7 Days", Description = "Standard delivery", Price = 10m },
            new DeliveryMethod { ShortName = "Express", DeliveryTime = "2-3 Days", Description = "Fast courier delivery", Price = 25m },
            new DeliveryMethod { ShortName = "Next Day", DeliveryTime = "1-2 Days", Description = "Next business day delivery", Price = 50m },
            new DeliveryMethod { ShortName = "Free", DeliveryTime = "7-10 Days", Description = "Free postal shipping", Price = 0m }
        );
        context.SaveChanges();
    }
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGroup("api").MapIdentityApi<AppUser>();
app.MapHub<NotificationHub>("/hub/notifications");

app.Run();
