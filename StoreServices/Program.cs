using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("Policies",
        builder => builder.WithOrigins(
            "https://localhost:44371",
            "https://localhost:5174", 
            "http://localhost:5174", 
            "http://192.168.0.109", 
            "http://localhost:5173",
            "https://localhost:5173",
            "https://192.168.0.109").AllowAnyMethod().AllowAnyHeader().AllowCredentials());
});
var app = builder.Build();
app.UseCors("Policies");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapControllers();
app.Run();