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
using Stripe;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);


DotNetEnv.Env.Load();
// JWT Configuration
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);
builder.Services.AddSingleton<EmailValidationService>();
builder.Services.AddSingleton<JwtHelper>();
builder.Services.AddSingleton<EmailService>();
builder.Services.AddScoped<IUserService,UserService>();
builder.Services.AddSingleton<PaymentService>();
builder.Services.AddHttpContextAccessor();
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
		},
		OnMessageReceived = context =>
		{
			// 👇 Read token from 'authToken' cookie
			var tokenFromCookie = context.HttpContext.Request.Cookies["authToken"];
			if (!string.IsNullOrEmpty(tokenFromCookie))
			{
				context.Token = tokenFromCookie;
			}
			return Task.CompletedTask;
		}
	};
});
builder.Services.AddSwaggerGen(options =>
{
	options.SwaggerDoc("v1", new OpenApiInfo
	{
		Title = "IdealTrip API",
		Version = "v1",
		Description = "API documentation for the IdealTrip project",
	});
});

builder.Services.AddAuthorization();
// Add services to the container.
builder.Services.AddControllers();
builder.Logging.AddConsole();
builder.Configuration.AddEnvironmentVariables();
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowMultipleOrigins", builder =>
		builder
			.WithOrigins(
				"http://127.0.0.1:5500",
				"https://127.0.0.1:5500",
				"http://localhost:3000",
				"https://localhost:3000",
				"http://localhost:3001",
				"https://localhost:3001",
				"https://localhost:7216",
				"http://localhost:7216"
			)
			.AllowAnyHeader()
			.AllowAnyMethod()
			.AllowCredentials() // ? Important for WebSockets
	);
});



var stripeConfig = builder.Configuration.GetSection("Stripe");
StripeConfiguration.ApiKey = stripeConfig["SecretKey"];
string azureBlobStorageConnectionString = builder.Configuration["AzureBlobStorage:ConnectionString"];

// Register the BlobServiceClient with the connection string
builder.Services.AddSingleton(x => new BlobServiceClient(azureBlobStorageConnectionString));
builder.Services.AddSignalR();
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
app.MapHub<NotificationHub>("/notificationhub");

app.UseHttpsRedirection();
app.UseCors("AllowMultipleOrigins");
app.UseWebSockets();
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
