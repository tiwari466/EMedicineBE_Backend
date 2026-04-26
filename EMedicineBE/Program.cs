using EMedicineBE.Data.Repositories;
using EMedicineBE.Services.Interfaces;
using EMedicineBE.Services.Implementations;
using QuestPDF.Infrastructure;

QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// =======================
// DI Services
// =======================
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IMedicineRepository, MedicineRepository>();
builder.Services.AddScoped<IMedicineService, MedicineService>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<ICartService, CartService>();

// =======================
// Controllers + Swagger
// =======================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// =======================
// ✅ CORS (React Frontend)
// =======================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000") // React app
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// =======================
// Middleware
// =======================
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ❌ Keep disabled if causing issues locally
// app.UseHttpsRedirection();

// ✅ Apply CORS BEFORE Authorization
app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

app.Run();