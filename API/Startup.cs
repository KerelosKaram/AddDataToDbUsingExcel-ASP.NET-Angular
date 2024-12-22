using System.Text;
using API.Data.AppDbContext.DbElWagd;
using API.Data.AppDbContext.Identity;
using API.Data.AppDbContext.OneNineTwo;
using API.Data.AppDbContext.Sql2017DbContext;
using API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace MessagingApp
{
    public class Startup
    {
        private readonly IConfiguration _config;
        public Startup(IConfiguration config)
        {
            _config = config;
        }

        // This method is used to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            // Add your database contexts
            services.AddDbContext<IdentityDbContext>(options =>
            {
                options.UseSqlServer(_config.GetConnectionString("IdentityConnection"));
            });

            services.AddDbContext<Sql2017DbContext>(options =>
            {
                options.UseSqlServer(_config.GetConnectionString("SQL2017Connection"));
            });

            services.AddDbContext<OneNineTwoDbContext>(options =>
            {
                options.UseSqlServer(_config.GetConnectionString("OneNineTwoConnection"));
            });

            services.AddDbContext<DbElWagdDbContext>(options =>
            {
                options.UseSqlServer(_config.GetConnectionString("DbElWagd"));
            });

            // Add CORS policy
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAngularApp", builder =>
                {
                    builder.WithOrigins("https://localhost:4200")  // The URL of your Angular app
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });

            // Configure JWT Authentication
            var jwtSettings = _config.GetSection("JwtSettings");
            var keyString = jwtSettings["Key"];
            if (string.IsNullOrEmpty(keyString))
            {
                throw new ArgumentNullException("JWT key is not configured.");
            }
            var key = Encoding.UTF8.GetBytes(keyString); // Secret key

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings["Issuer"], // Your app's issuer
                        ValidAudience = jwtSettings["Audience"], // Your app's audience
                        IssuerSigningKey = new SymmetricSecurityKey(key) // Signing key
                    };
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("HasSmsClaim", policy =>
                {
                    policy.RequireClaim("SMS");
                    policy.RequireClaim("IGPItemElamir");
                    policy.RequireClaim("IGPSalesmanTarget");
                    policy.RequireClaim("PSKUItemElamir");
                    policy.RequireClaim("QSCustomerBrandTarget");
                    policy.RequireClaim("QSCustomerTarget");
                });
            });

            // Registering the IHttpContextAccessor service
            services.AddHttpContextAccessor();

            // Register other services
            services.AddScoped<ExcelImportService>(); // Register the service

            // Add controllers
            services.AddControllers();

            // Add Swagger and API Explorer services
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
        }

        // This method is used to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IHostEnvironment environment)
        {
            // Configure middleware
            if (environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors("AllowAngularApp");

            app.UseHttpsRedirection();

            // Define routing using UseEndpoints
            app.UseRouting(); // Enables routing

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                // Map the /weatherforecast route
                endpoints.MapControllers();
            });
        }
    }
}
