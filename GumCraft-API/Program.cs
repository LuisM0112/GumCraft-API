using GumCraft_API.Database;
using GumCraft_API.Models.Database;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace GumCraft_API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            //Añadir dbContext al servicio de inyección de dependencias
            builder.Services.AddScoped<MyDbContext>();
            builder.Services.AddTransient<DbSeeder>();




            builder.Services.AddAuthentication()
                .AddJwtBearer(options =>
                {

                    string key = Environment.GetEnvironmentVariable("JWT_KEY");

                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
                    };

                });
            // Permite CORS
            if (builder.Environment.IsDevelopment())
            {
                builder.Services.AddCors(options =>
                {
                    options.AddDefaultPolicy(builder =>
                    {
                        builder.SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost")
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    });
                });
            }

            var app = builder.Build();

            //Si no está creada la base de datos, la creamos y la rellenamos
            /*using (IServiceScope scope = app.Services.CreateScope())
            {
                DbSeeder dbSeeder = scope.ServiceProvider.GetService<DbSeeder>();
                await dbSeeder.SeedAsync();
            }*/

            //Creamos un scope 
            using (IServiceScope scope = app.Services.CreateScope())
            {
                MyDbContext dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();
                //dbContext.Database.EnsureCreated();
                DbSeeder dbSeeder = scope.ServiceProvider.GetService<DbSeeder>();
                await dbSeeder.SeedAsync();
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();

                app.UseCors();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
