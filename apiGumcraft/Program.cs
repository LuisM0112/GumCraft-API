using GumcraftApi.Database;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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
            builder.Services.AddAuthentication().AddJwtBearer(options =>
            {
                try
                {
                    string key = Environment.GetEnvironmentVariable("JWT-KEY");

                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))

                    };
                }catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
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

                // Permite CORS
                app.UseCors();

            }
            app.UseHttpsRedirection();

            app.UseAuthorization();
            app.UseAuthentication();

            app.MapControllers();

            app.Run();
        }
    }
}
