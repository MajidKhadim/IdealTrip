using IdealTrip.Helpers;
using IdealTrip.Models;
using IdealTrip.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;
using DotNetEnv;
using System.Security.Claims;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using System.Net.NetworkInformation;

var builder = WebApplication.CreateBuilder(args);


DotNetEnv.Env.Load();
// JWT Configuration
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);
builder.Services.AddSingleton<JwtHelper>();
builder.Services.AddSingleton<EmailService>();
builder.Services.AddScoped<IUserService,UserService>();

builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = true,
		ValidateAudience = true,
		ValidateLifetime = true,
		ValidateIssuerSigningKey = true,
		ValidIssuer = jwtSettings["Issuer"],
		ValidAudience = jwtSettings["Audience"],
		IssuerSigningKey = new SymmetricSecurityKey(key),
		RoleClaimType = ClaimTypes.Role
	};
	options.Events = new JwtBearerEvents
	{
		OnAuthenticationFailed = context =>
		{
			if (context.Exception is SecurityTokenExpiredException)
			{
				context.Response.Headers.Add("Token-Expired", "true");
			}
			return Task.CompletedTask;
		}
	};
});
// Add services to the container.
builder.Services.AddControllers();
builder.Logging.AddConsole();
builder.Configuration.AddEnvironmentVariables();
builder.Services.AddCors(options => options.AddPolicy("AllowAnyOrigin",
	builder => builder.AllowAnyOrigin()
	.AllowAnyHeader()
	.AllowAnyMethod()));
string azureBlobStorageConnectionString = builder.Configuration["AzureBlobStorage:ConnectionString"];

// Register the BlobServiceClient with the connection string
builder.Services.AddSingleton(x => new BlobServiceClient(azureBlobStorageConnectionString));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
	options.UseSqlServer(builder.Configuration["ENV_IDEALTRIPDBCONNECTION"]);
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
{
	options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_?&@.!$*+,. ";
	options.User.RequireUniqueEmail = true;
	options.Password.RequireNonAlphanumeric = false;
	options.Password.RequireDigit = true;
	options.Password.RequireLowercase = true;
	options.Password.RequireUppercase = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}
app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseCors("AllowAnyOrigin");
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
