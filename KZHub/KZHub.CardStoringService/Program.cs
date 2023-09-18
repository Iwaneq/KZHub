
using KZHub.CardStoringService.Data;
using Microsoft.EntityFrameworkCore;

namespace KZHub.CardStoringService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            if (builder.Environment.IsDevelopment())
            {
                Console.WriteLine("--> Using InMemory Database");
                builder.Services.AddDbContext<DataContext>(options =>
                {
                    options.UseInMemoryDatabase("InMem");
                });
            }
            else
            {
                Console.WriteLine("--> Using SQL Server Database");
                builder.Services.AddDbContext<DataContext>(options =>
                {
                    options.UseSqlServer(builder.Configuration.GetConnectionString("CardsDB"));
                });
            }

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