using DemoProject.Helper;
using DemoProject.Validation;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Models.Book;
using Models.RequestModel;
using Models.SpDbContext;
using Serilog;
using Service.Repository.Implementation;
using Service.Repository.Interface;

namespace DemoProject;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        
       // builder.Services.AddScoped<IValidator<BookRequestModel>, BookValidator>();
        
        //connection String
        // Get connection string from appsettings.json
        var connectionString = builder.Configuration.GetConnectionString("DBConnection");
        //Project Dependency Injection
        
        
        // Add services to the container.
        builder.Services.AddScoped<IBookRepository, BookRepository>();
        
        //serilogger
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("log.txt",
                rollingInterval: RollingInterval.Day,
                rollOnFileSizeLimit: true)
            .CreateLogger();
        //Fluent Validation
        builder.Services.AddControllers()
        
            .AddFluentValidation(fv =>
        
            {
        
                fv.RegisterValidatorsFromAssembly(AppDomain.CurrentDomain.GetAssemblies()
        
                    .SingleOrDefault(assembly => assembly.GetName().Name == typeof(Program).Assembly.GetName().Name));
        
            });
        builder.Services.AddDbContext<BookDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlServerOptionsAction: sqlOptions =>
            {
        
            });
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            options.EnableSensitiveDataLogging(true);
        }, ServiceLifetime.Transient);

        builder.Services.AddDbContext<LibraryManagementSpContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlServerOptionsAction: sqlOptions =>
            {

                sqlOptions.EnableRetryOnFailure();

            });
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            options.EnableSensitiveDataLogging(true);
        }, ServiceLifetime.Transient);
        // Register the UserService
        UnitOfWorkServiceCollectionExtentions.AddUnitOfWork<BookDbContext>(builder.Services);
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        app.UseMiddleware<ExceptionMiddleware>();
        app.UseHttpsRedirection();

        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}