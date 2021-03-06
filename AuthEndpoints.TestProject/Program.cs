using AuthEndpoints;
using AuthEndpoints.MinimalApi;
using AuthEndpoints.TestProject.Data;
using AuthEndpoints.TestProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<MyDbContext>(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        options.UseSqlite(builder.Configuration.GetConnectionString("DataSQLiteConnection"));
    }
});

builder.Services.AddIdentityCore<MyApplicationUser>(option =>
{
    option.User.RequireUniqueEmail = true;
    option.Password.RequireDigit = false;
    option.Password.RequireNonAlphanumeric = false;
    option.Password.RequireUppercase = false;
    option.Password.RequiredLength = 0;
})
    .AddEntityFrameworkStores<MyDbContext>()
    .AddDefaultTokenProviders();

builder.Services
    .AddAuthEndpoints<string, MyApplicationUser>(options =>
    {
        options.Issuer = "https://localhost:7004";
        options.Audience = "http://localhost:3000";
        options.EmailConfirmationUrl = "http://localhost:3000/email_confirm/{uid}/{token}";
        options.PasswordResetUrl = "http://localhost:3000/password_reset/{uid}/{token}";
        options.EmailOptions = new EmailOptions()
        {
            From = Environment.GetEnvironmentVariable("GOOGLE_MAIL_APP_USER")!,
            Host = "smtp.gmail.com",
            Port = 587,
            User = Environment.GetEnvironmentVariable("GOOGLE_MAIL_APP_USER")!,
            Password = Environment.GetEnvironmentVariable("GOOGLE_MAIL_APP_PASSWORD")!,
        };
    })
    .AddAllEndpointDefinitions()
    .AddJwtBearerAuthScheme();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.WithOrigins("http://localhost:3000")
                .AllowAnyHeader();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseReDoc(c =>
    {
        c.RoutePrefix = "docs";
    });
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapAuthEndpoints();

app.Run();
