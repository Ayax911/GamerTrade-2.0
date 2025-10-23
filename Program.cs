using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


builder.Services.AddScoped<APiGamer.Repositorio.Abstracciones.IConexionFactory, APiGamer.Repositorio.SqlServerConexionFactory>();

builder.Services.AddScoped<APiGamer.Servicios.Abstracciones.IProvedor, APiGamer.Servicios.Provedor>();
builder.Services.AddScoped<APiGamer.Repositorio.Abstracciones.IRepositorioConsulta, APiGamer.Repositorio.RepositorioConsulta>();
builder.Services.AddScoped<APiGamer.Servicios.Abstracciones.IServicioConsultas, APiGamer.Servicios.ServicioConsultas>();

builder.Services.AddScoped<APiGamer.Repositorio.Abstracciones.IRepositorioLectura, APiGamer.Repositorio.RepositorioLectura>();
builder.Services.AddScoped<APiGamer.Servicios.Abstracciones.IServiciosCrud, APiGamer.Servicios.ServiciosCrud>();



var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:ClaveSuperSecretaDeMasDe32caracteres"]!);

builder.Services.AddAuthentication(option =>
{
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
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
           IssuerSigningKey = new SymmetricSecurityKey(key)
       };
   });

builder.Services.AddScoped<APiGamer.Servicios.Abstracciones.ITokenServices, APiGamer.Servicios.TokenServices>();





builder.Services.AddEndpointsApiExplorer();   // <--- FALTA AQU�
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowAll");
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
