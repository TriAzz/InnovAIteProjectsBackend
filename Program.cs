using innovaite_projects_dashboard;
using innovaite_projects_dashboard.Persistence;
using System.Reflection;
using System.IO;
using System.Text.Json;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.WithOrigins(
                "http://localhost:3000",
                "https://innovaite-projects-dashboard.netlify.app",
                "https://innovaite-projects-dashboard-frontend.netlify.app"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
        });
});

//USE Advanced ORM implementation (MongoDB)
builder.Services.AddScoped<IUserDataAccess, UserMongoDB>();
builder.Services.AddScoped<IProjectDataAccess, ProjectMongoDB>();
builder.Services.AddScoped<ICommentDataAccess, CommentMongoDB>();
builder.Services.AddScoped<DashboardContext>();

builder.Services.AddAuthentication("BasicAuthentication").AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireClaim(ClaimTypes.Role, "Admin"));
    options.AddPolicy("UserOnly", policy => policy.RequireClaim(ClaimTypes.Role, "Admin", "User"));
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "InnovAIte Projects Dashboard API",
        Description = "Backend service that provides resources for the InnovAIte Projects Dashboard application.",
        Contact = new OpenApiContact
        {
            Name = "InnovAIte Administrator",
            Email = "admin@innovaite.com"
        },
    });
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"; 
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

var app = builder.Build();

app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(setup => setup.InjectStylesheet("/styles/dashboard.css"));
}

// Use CORS middleware
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

app.MapControllers(); 

app.Run();
