using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddScoped<APiGamer.Servicios.Abstracciones.IProvedor, APiGamer.Servicios.Provedor>();
builder.Services.AddScoped<APiGamer.Repositorio.Abstracciones.IRepositorioConsulta, APiGamer.Repositorio.RepositorioConsulta>();
builder.Services.AddScoped<APiGamer.Servicios.Abstracciones.IServicioConsultas, APiGamer.Servicios.ServicioConsultas>();

builder.Services.AddEndpointsApiExplorer();   // <--- FALTA AQUÍ
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

app.UseAuthorization();

app.MapControllers();

app.Run();
