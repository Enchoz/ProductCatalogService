using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProductService.API.Middleware;
using ProductService.Infrastructure.Configration;
using ProductService.Infrastructure.Configuration;
using System.Text;
using Microsoft.OpenApi.Models;
using System.Reflection;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using Microsoft.Extensions.Options;
using Microsoft.Data.SqlClient;
using System.Configuration;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(builder.Configuration.GetValue<string>("ElasticSearch:Url")))
    {
        AutoRegisterTemplate = true,
        IndexFormat = "product-service-logs-{0:yyyy.MM.dd}"
    })
    .CreateLogger();

builder.Host.UseSerilog();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Product API", Version = "v1" });
    c.EnableAnnotations();

    // Enable XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var retries = 5;

//for (var i = 0; i < retries; i++)
//{
//    try
//    {
//        using var connection = new SqlConnection(connectionString);
//        connection.Open();
//        break; // Break if connection succeeds
//    }
//    catch (SqlException)
//    {
//        if (i == retries - 1) throw; // Re-throw on the last         retry
//        Thread.Sleep(5000); // Wait before retrying
//    }
//    catch(Exception ex)
//    {
//        Console.WriteLine($"{JsonConvert.SerializeObject(ex.InnerException)}");
//        Console.WriteLine($"Unable to connect to {connectionString}");
//    }
//}


builder.Services.AddDbContextPool<ProductDbContext>(
    options =>
               options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
            sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                   errorNumbersToAdd: new[] { 1205 });
            }
    )
);

//builder.Services.AddDbContextPool<ProductDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
//);



builder.Services.ConfigureInfrastructureService(builder.Configuration);

// Add controllers and FluentValidation support
builder.Services.AddControllers().AddFluentValidation(fv =>
    fv.RegisterValidatorsFromAssemblyContaining<Program>());

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = "https://localhost:5001";
                options.Audience = "api1";
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "yourdomain.com",
                    ValidAudience = "yourdomain.com",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("your_secret_key"))
                };
            });

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetValue<string>("Redis:Configuration");
    options.InstanceName = builder.Configuration.GetValue<string>("Redis:InstanceName");
});

var app = builder.Build();

app.UseSerilogRequestLogging();

app.Urls.Add("http://0.0.0.0:8080"); // Explicitly bind to port 8080

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

using (var serviceScope = app.Services.CreateScope())
{
    var context = serviceScope.ServiceProvider.GetRequiredService<ProductDbContext>();
    context.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Product API V1"));
app.UseDeveloperExceptionPage();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //using (var scope = app.Services.CreateScope())
    //{
    //    var dbContext = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
    //    dbContext.Database.EnsureCreated();
    //}

}

//app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


app.Run();