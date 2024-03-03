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

            // Configuramos para que el directorio de trabajo sea donde está el ejecutable
            Directory.SetCurrentDirectory(AppContext.BaseDirectory);

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

                // Permite CORS
                app.UseCors(config => config
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .SetIsOriginAllowed(origin => true)
                    .AllowCredentials());
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            // Habilitamos el uso de archivos estáticos
            app.UseStaticFiles();

            app.Run();
        }
    }
}
