using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ResultsService;

namespace ResultsService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDbContext<ResultsServiceContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("ResultsServiceContext") ?? throw new InvalidOperationException("Connection string 'ResultsServiceContext' not found.")));
            //Server=tcp:msc-results.database.windows.net,1433;Initial Catalog=MSCResults;Persist Security Info=False;User ID=MSCAdmin;Password=FudgeBriask1234$;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddDbContext<ResultsServiceContext>(options =>
                options.UseSqlServer("Server=tcp:msc-results.database.windows.net,1433;Initial Catalog=MSCResults;Persist Security Info=False;User ID=MSCAdmin;Password=FudgeBriask1234$;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"));

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
