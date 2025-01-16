using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
}).AddControllersAsServices();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Policies",
        builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
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