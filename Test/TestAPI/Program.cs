using ExcelDataReader;
using System.Data;
using Microsoft.Extensions.Logging;

namespace TestAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //return;

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            /// log4net logger
            builder.Logging.ClearProviders();
            builder.Logging.AddLog4Net();
            builder.Logging.AddConsole();

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}