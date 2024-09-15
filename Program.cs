using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Ajout des contrôleurs et de l'explorateur d'API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configuration CORS pour permettre les requêtes depuis l'application Angular
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp",
        corsBuilder =>
        {
            corsBuilder.WithOrigins("http://localhost:82")  // URL du frontend Angular
                       .AllowAnyHeader()
                       .AllowAnyMethod()
                       .AllowCredentials();
        });
});

// Configuration JWT pour l'authentification
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:cle"]))
        };
    });

var app = builder.Build();

// Servir les fichiers statiques (pour le frontend Angular)
app.UseStaticFiles();

// Configuration du routage
app.UseRouting();

// Application des politiques CORS
app.UseCors("AllowAngularApp");

// Middleware d'authentification et d'autorisation
app.UseAuthentication();
app.UseAuthorization();

// Mappage des routes API
app.MapControllers();

// Rediriger vers l'index.html d'Angular pour les routes non gérées par l'API
app.MapFallbackToFile("index.html");

// Lancement de l'application
app.Run();
