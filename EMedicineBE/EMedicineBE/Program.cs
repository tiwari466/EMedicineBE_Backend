var builder = WebApplication.CreateBuilder(args);

// ✅ Load Railway / ENV vars first
builder.Configuration.AddEnvironmentVariables();

// ✅ PostgreSQL connection
var cs = builder.Configuration.GetConnectionString("PostgresCS");

// ✅ Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

// ✅ Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection(); // ❌ Railway handles SSL

app.UseCors("AllowReact");
app.UseAuthorization();
app.UseStaticFiles();
app.MapControllers();
app.Run();
