using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MoviesApi.Data;
using MoviesApi.Helpers;
using MoviesApi.Seeds;
using MoviesApi.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddIdentity<ApplicationUser,IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseSqlServer(connectionString));

builder.Services.AddScoped<IAuthService, AuthService>();

//JWTSETUP
builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
			   .AddJwtBearer(o =>
			   {
				   o.RequireHttpsMetadata = false;
				   o.SaveToken = false;
				   o.TokenValidationParameters = new TokenValidationParameters
				   {
					   ValidateIssuerSigningKey = true,
					   ValidateIssuer = true,
					   ValidateAudience = true,
					   ValidateLifetime = true,
					   ValidIssuer = builder.Configuration["JWT:Issuer"],
					   ValidAudience = builder.Configuration["JWT:Audience"],
					   IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]))
				   };
			   });

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors();
builder.Services.AddSwaggerGen();
builder.Services.Configure<JWT>(builder.Configuration.GetSection("JWT"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(c => c.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

app.UseAuthentication();

app.UseAuthorization();

var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
using var scope = scopeFactory.CreateScope();

var RoleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

await DefaultRoles.SeedRoles(RoleManager);

app.MapControllers();

app.Run();
