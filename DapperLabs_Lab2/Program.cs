using DapperLabs_Lab2;
using DapperLabs_Lab2.Repositories;

var builder = WebApplication.CreateBuilder(args);

// [API] Add services to the container - API dependency injection configuration
builder.Services.AddControllers(); // [API] Register MVC controllers
builder.Services.AddEndpointsApiExplorer(); // [API] API explorer for Swagger
// [API] Swagger configuration - Interactive API documentation
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "Dapper Labs API", 
        Version = "v1",
        Description = "A .NET 8 Web API demonstrating Dapper ORM with SQL Server"
    });
    
    // [API] Include XML comments for better Swagger documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// [API] Logging configuration
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// [DAPPER] Get connection string - Dapper uses connection strings directly
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// [API] [DAPPER] Register repositories with DI - Pass connection string to Dapper repositories
builder.Services.AddScoped<ICustomerRepository>(sp => new CustomerRepository(connectionString));
builder.Services.AddScoped<IInvoiceRepository>(sp => new InvoiceRepository(connectionString));
builder.Services.AddScoped<ITelephoneNumberRepository>(sp => new TelephoneNumberRepository(connectionString));

// [API] CORS policy for development - Allow cross-origin requests
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// [DAPPER] Initialize database and seed data - Manual schema management (no EF migrations)
using (var scope = app.Services.CreateScope())
{
    var initLogger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var seedSettings = builder.Configuration.GetSection("SeedSettings").Get<SeedSettings>() ?? new SeedSettings();

    try
    {
        initLogger.LogInformation("Initializing database...");
        // [DAPPER] Manual database initialization - Execute CREATE TABLE scripts
        await DatabaseInitializer.InitializeDatabaseAsync(connectionString);

        if (seedSettings.EnableSeeding)
        {
            initLogger.LogInformation("Seeding database...");
            // [DAPPER] Data seeding with INSERT statements
            await BogusDataGenerator.SeedDatabase(
                connectionString,
                customerCount: seedSettings.CustomerCount,
                minInvoicesPerCustomer: seedSettings.MinInvoicesPerCustomer,
                maxInvoicesPerCustomer: seedSettings.MaxInvoicesPerCustomer,
                minPhoneNumbersPerCustomer: seedSettings.MinPhoneNumbersPerCustomer,
                maxPhoneNumbersPerCustomer: seedSettings.MaxPhoneNumbersPerCustomer
            );
            initLogger.LogInformation("Database seeding completed");
        }
        else
        {
            initLogger.LogInformation("Database seeding is disabled in configuration");
        }
    }
    catch (Exception ex)
    {
        initLogger.LogError(ex, "An error occurred while initializing the database");
    }
}

// [API] Configure the HTTP request pipeline
// [API] Enable Swagger in all environments (not just Development)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dapper Labs API V1");
    c.RoutePrefix = string.Empty; // [API] Set Swagger UI at the app's root (http://localhost:5000)
});

// [API] Middleware pipeline configuration
app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthorization();

// [API] Map API controllers - Route HTTP requests to controller actions
app.MapControllers();

// [API] Log startup information with clear instructions
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("===========================================");
logger.LogInformation("Dapper Labs API Started Successfully!");
logger.LogInformation("===========================================");
logger.LogInformation("Swagger UI: http://localhost:5000");
logger.LogInformation("API Base URL: http://localhost:5000/api");
logger.LogInformation("===========================================");
logger.LogInformation("Open your browser and navigate to:");
logger.LogInformation("  http://localhost:5000");
logger.LogInformation("===========================================");

app.Run();
