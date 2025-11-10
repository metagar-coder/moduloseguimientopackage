using Microsoft.Extensions.Logging.EventLog;
using moduloseguimiento.API.Data;
using moduloseguimiento.API.Services.Interfaces;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using moduloseguimiento.API.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var MyAllowSpecificOrigins = "AllowOrigin";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
    policy =>
    {
        // PROD
        //policy.WithOrigins("http://localhost:4200")
        // QA
        //policy.WithOrigins("https://academicos.uv.mx/ApiEvalPosgrado")
        policy.WithOrigins(
            "http://localhost:4200"         
            )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");


// Register SqlConnection as a singleton service
builder.Services.AddSingleton<SqlConnection>(_ => new SqlConnection(connectionString));

// Register ApplicationDbContext
builder.Services.AddTransient<ApplicationDbContext>();


builder.Services.AddControllers().AddJsonOptions(opt =>
{
    opt.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    opt.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
});

builder.Services.AddTransient<IActiveDirectory, ActiveDirectoryService>();

//Agregar Servicios de Swagger
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "API8-MODULO SEGUIMIENTO", Version = "v1" }); //---

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidAudience = builder.Configuration["AUTH_JWT:JWT_AUDIENCE_TOKEN"],
        ValidIssuer = builder.Configuration["AUTH_JWT:JWT_ISSUER_TOKEN"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["AUTH_JWT:JWT_SECRET_KEY"])),
        RoleClaimType = "TipoPerfil"
    };
});

// Disable automatic model state validation
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

// Configuración de políticas de autorización basadas en roles
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Administrador", policy => policy.RequireRole("Administrador"));
    options.AddPolicy("MonitorAA", policy => policy.RequireRole("Área Académica (Monitor AA)"));
    options.AddPolicy("MonitorPE", policy => policy.RequireRole("Programa Educativo (Monitor PE)"));

    //Nueva politica combinada: Coordinador o Administrador
    /*options.AddPolicy("CoordinadorOAdministrador", policy => policy.RequireAssertion(context => context.User.IsInRole("Coordinador") || context.User.IsInRole("Administrador")));
    options.AddPolicy("UnidadRevisoraOAdministrador", policy => policy.RequireAssertion(context => context.User.IsInRole("Unidad Revisora") || context.User.IsInRole("Administrador")));
    options.AddPolicy("UnidadRevisoraOCoordinador", policy => policy.RequireAssertion(context => context.User.IsInRole("Unidad Revisora") || context.User.IsInRole("Coordinador")));
    options.AddPolicy("UnidadRevisoraOCoordinadorOAdministrador", policy => policy.RequireAssertion(context => context.User.IsInRole("Unidad Revisora") || context.User.IsInRole("Coordinador") || context.User.IsInRole("Administrador")));
    */
});

// Registrar HttpClient en el contenedor de servicios
builder.Services.AddHttpClient();

builder.Services.AddTransient<IEncrypt, EncryptService>();

builder.Services.AddTransient<IGetSPARHData, GetSPARHDataService>();

builder.Services.AddHostedService<IncidenciasSchedulerService>();
builder.Services.AddScoped<DeteccionIncidenciasAccesoService>();
builder.Services.AddScoped<DeteccionIncidenciasAsistenciaService>();
builder.Services.AddScoped<DeteccionIncidenciasActividadesService>();
builder.Services.AddScoped<DeteccionIncidenciasForosService>();


// Servicios para obtener el calendario, periodo y dias UV (DIAS DE DESCANSO COMO FINES DE SEMANA)
builder.Services.AddTransient<ICalendar, CalendarService>();

// Seguridad para la base de datos de Oracle
builder.Services.AddSingleton<SeguridadSIIUService>();

builder.Services.AddScoped<OracleService>();


// ------ ENVIO DE CORREOS ------------------------

builder.Services.AddTransient<IServiceEmail, EmailService>();

//---------------------------------------------------

builder.Services.AddHttpContextAccessor();
builder.Logging.AddConsole();

var app = builder.Build();

app.UseCors(MyAllowSpecificOrigins);

app.UseAuthentication(); // <- Muy importante si usas JWT

#region Swagger_Desarrollo/Test
/*app.UseSwagger();
app.UseSwaggerUI(c =>
{

    if (app.Environment.IsDevelopment())
    {
        // Local
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API8-MODULOSEGUIMIENTO v1");
    }
    else
    {
        // Producción
        //c.SwaggerEndpoint("/apieminus/swagger/v1/swagger.json", "API8-MODULOSEGUIMIENTO v1");

        // Test
        c.SwaggerEndpoint("/apieminusdev/swagger/v1/swagger.json", "API8-MODULOSEGUIMIENTO v1");
        c.RoutePrefix = string.Empty; // Para que Swagger esté en https://tuservidor.com/
    }

});*/
#endregion

// Swagger SOLO en desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API8-MODULOSEGUIMIENTO v1");
    });
}

app.UseAuthorization();

app.MapControllers();

app.Run();