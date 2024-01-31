
using GumcraftApi.Database;

namespace apiGumcraft
{
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
            //Añadir dbContext al servicio de inyección de dependencias
            builder.Services.AddScoped<MyDbContext>();

            var app = builder.Build();

            //Creamos un scope 
            using (IServiceScope scope = app.Services.CreateScope())
            {
                MyDbContext dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();
                dbContext.Database.EnsureCreated();
            }

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
