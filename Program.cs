using BackendOrar.Data;
using BackendOrar.Models;
using BackendOrar.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

/* Binds the PostgreSQL database to the entities. */
builder.Services.AddEntityFrameworkNpgsql().AddDbContext<OrarContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("BackendOrarDB")));

/* get the immutable copy of appSettings section */
builder.Services.Configure<AppSettings>(
        builder.Configuration.GetSection(nameof(AppSettings)));

/* register AppSettings singleton service */
builder.Services.AddSingleton<IAppSettings>(sp =>
    sp.GetRequiredService<IOptions<AppSettings>>().Value);

/* AdminSettings configuration section */
builder.Services.Configure<AdminSettings>(
        builder.Configuration.GetSection(nameof(AdminSettings)));

/* register AdminSettings as singleton service */
builder.Services.AddSingleton<IAdminSettings>(sp =>
    sp.GetRequiredService<IOptions<AdminSettings>>().Value);

/* create an immutable copy of the JwtSettings section */
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection(nameof(JwtSettings)));

/* register jwt settings singleton service */
builder.Services.AddSingleton<IJwtSettings>(sp =>
    sp.GetRequiredService<IOptions<JwtSettings>>().Value);

/* add rate limiting services */
builder.Services.AddRateLimiter(options =>
{
    /* create named policies */
    options.AddFixedWindowLimiter("DefaultPolicy", fixedWindowOptions =>
    {
        fixedWindowOptions.PermitLimit = 20;
        fixedWindowOptions.Window = TimeSpan.FromMinutes(1);
        fixedWindowOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        fixedWindowOptions.QueueLimit = 0;
    });

    /* policy for authenticated users */
    options.AddFixedWindowLimiter("AuthenticatedPolicy", fixedWindowOptions =>
    {
        fixedWindowOptions.PermitLimit = 20;
        fixedWindowOptions.Window = TimeSpan.FromMinutes(1);
        fixedWindowOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        fixedWindowOptions.QueueLimit = 0;
    });

    /* stricter policy for specific endpoints */
    options.AddFixedWindowLimiter("StrictPolicy", fixedWindowOptions =>
    {
        fixedWindowOptions.PermitLimit = 10;
        fixedWindowOptions.Window = TimeSpan.FromMinutes(1);
        fixedWindowOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        fixedWindowOptions.QueueLimit = 0;
    });

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.", token);
    };
});

var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();

if (jwtSettings != null)
{
    bool validateLifetime = jwtSettings.ValidateLifetime != null 
        && jwtSettings.ValidateLifetime.Value;

    bool useHttps = (jwtSettings.UseHttpsServerAsDefault != null
        && jwtSettings.UseHttpsServerAsDefault.Value);

    string usedServer = useHttps ? jwtSettings.HttpsServer 
        : jwtSettings.HttpServer;

    /* register JWT token based authentication for usage in the application */
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = validateLifetime,
            ValidateIssuerSigningKey = true,
            ValidIssuer = usedServer,
            ValidAudience = usedServer,
            IssuerSigningKey = new SymmetricSecurityKey(
                Convert.FromBase64String(
                    jwtSettings.SigningKey))
        };
    });

    builder.Services.AddCors(options =>
    {
        options.AddPolicy(name: "AllowOrigin",
            builder =>
            {
                builder.WithOrigins(jwtSettings.HttpsServer, jwtSettings.HttpServer)
                                    .AllowAnyHeader()
                                    .AllowAnyMethod();
            });
    });
}

/* declares the crypto service, required to the password encryption */
builder.Services.AddSingleton<ICryptoService, CryptoService>();

/* declares the jwt token service required to the account authentication and authorization */
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();

/* add the admin service, required for email sending */
builder.Services.AddSingleton<IAdminService, AdminService>();

/* register the account service, with all features */
builder.Services.AddTransient<IUserService, UserService>();

/* register the course service, with all features */
builder.Services.AddTransient<ICourseService, CourseService>();

/* register the professor service, with all features */
builder.Services.AddTransient<IProfessorService, ProfessorService>();

/* register the classroom service, with all features */
builder.Services.AddTransient<IClassroomService, ClassroomService>();

/* register the group service, with all features */
builder.Services.AddTransient<IGroupService, GroupService>();

/* register the time table service, with all features */
builder.Services.AddTransient<ITimetableService, TimetableService>();

builder.Services.AddTransient<IMetadataService, MetadataService>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();
app.UseCors("AllowOrigin");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "BackendOrar V1.0.0");
        options.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
