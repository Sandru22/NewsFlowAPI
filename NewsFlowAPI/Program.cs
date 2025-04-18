using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using NewsFlowAPI.Models;
using NewsFlowAPI.Classifier;
using System.Text;
using NewsFlowAPI.Classifier;

var builder = WebApplication.CreateBuilder(args);

// 1. Ad?ug?m controlere
builder.Services.AddControllers();

// 2. Configur?m DbContext
builder.Services.AddDbContext<NewsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. Configur?m Identity pentru autentificare
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<NewsDbContext>()
    .AddDefaultTokenProviders();

// 4. Configur?m autentificarea JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // Dezactiveaz? pentru testare local?
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("super_secret_key")),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
    });

// 5. Activ?m autorizarea
builder.Services.AddAuthorization();

// 6. Ad?ug?m Swagger pentru testare API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 7. Ad?ug?m serviciile pentru procesarea ?tirilor
builder.Services.AddSingleton<RssService>();
builder.Services.AddSingleton<NewsClassifier>();
builder.Services.AddScoped<NewsProcessorService>();
builder.Services.AddHostedService<NewsBackgroundService>();

var app = builder.Build();

// 8. Configur?m pipeline-ul middleware

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Activ?m autentificarea ?i autorizarea
app.UseAuthentication();
app.UseAuthorization();

// Map?m controlerele
app.MapControllers();

// 9. Pornim aplica?ia
app.Run();