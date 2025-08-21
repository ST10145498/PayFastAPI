// FILE: Program.cs
using PayFastAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS (allow your app + website domains)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowKnownClients", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:3000",
                "http://localhost:4200",
                "http://10.0.2.2:8080",     // Android emulator -> host
                "https://your-website.example", // replace with your site
                "https://your-app-host.example" // replace if needed
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Services
builder.Services.AddSingleton<TransactionService>();
builder.Services.AddScoped<PayFastService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors("AllowKnownClients");

app.MapControllers();

app.Run();
