using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using ResortBooking.API.Configuration;
using ResortBooking.API.Data;
using ResortBooking.API.Dtos;
using ResortBooking.API.Infrastructure;
using ResortBooking.API.Models;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
if (jwtSettings!=null && string.IsNullOrWhiteSpace(jwtSettings.Secret))
{
    throw new InvalidOperationException("Jwt Secret is not Configured");
}
var key = Encoding.ASCII.GetBytes(jwtSettings.Secret);

// Add services to the container.
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

builder.RegisterDbContext();
builder.Services.AddApiVersioning(option =>
{
    option.AssumeDefaultVersionWhenUnspecified = true;
    option.DefaultApiVersion = new ApiVersion(1, 0);
    option.ReportApiVersions = true;
}).AddApiExplorer(option =>
{
    option.GroupNameFormat = "'v'VVV";
    option.SubstituteApiVersionInUrl = true;
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<ApplicationContext>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience=jwtSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddControllers();

var builderProvider = builder.Services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();
foreach (var description in builderProvider.ApiVersionDescriptions)
{
    var versionname = description.GroupName;
    var versionnumber = description.ApiVersion;
    var displayName = $"Demo API -- {versionname}";
    builder.Services.AddOpenApi(versionname,options =>
    {
        options.AddDocumentTransformer((document, context, cancellationToken) =>
        {
            document.Info = new OpenApiInfo
            {
                Title = "Demo Resort Api",
                Version=versionname,
                Description=displayName,
                Contact=new OpenApiContact
                {
                    Name="Abhiroop",
                    Email="abhiroop@gmail.com"
                }
            };
            document.Components ??= new();
            document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
            {
                ["Bearer"] = new OpenApiSecurityScheme()
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Enter Jwt Bearer Token"
                }
            };
            document.Security = [
                    new OpenApiSecurityRequirement{
                    {new OpenApiSecuritySchemeReference("Bearer"),new List<string>() }
                }
                ];
            return Task.CompletedTask;
        });

    });
}
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi



builder.Services.AddAutoMapper(o =>
{
    o.CreateMap<Villa, CreateVillaDto>().ReverseMap();
    o.CreateMap<Villa,VillaDetailsDto>().ReverseMap();
    o.CreateMap<Villa, UpdateVillaDto>();
    o.CreateMap<UpdateVillaDto, Villa>()
    .ForMember(
        dest => dest.ImageUrl,
        opt => opt.Ignore()
     );
    o.CreateMap<ApplicationUser, UserDto>().ReverseMap();
    o.CreateMap<VillaAmenities, AmenitiesDetailsDto>()
    .ForMember(
        dest=>dest.VillaName,
        opt=>opt.MapFrom(src=>src.Villa!=null?src.Villa.Name:null)
    );
    o.CreateMap<AmenitiesDetailsDto, VillaAmenities>();
    o.CreateMap<VillaAmenities, CreateAmenitiesDto>().ReverseMap();
    o.CreateMap<VillaAmenities, UpdateAmenitiesDto>().ReverseMap();
});
builder.RegisterAuthService();
builder.RegisterImageService();
builder.RegisterTokenService();
builder.RegisterVillaService();
builder.RegisterAmenitiesService();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi("/openapi/{documentName}.json");
    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    app.MapScalarApiReference(option =>
    {
        option.Title = "Demo Resort Booking API";
        var sortedVersions = provider.ApiVersionDescriptions.OrderBy(v => v.ApiVersion).ToList();
        foreach(var description in sortedVersions)
        {
            var versionname = description.GroupName;
            var versionnumber = description.ApiVersion;
            var displayName = $"Demo API -- {versionname}";
            var isDefault = description.ApiVersion.Equals(new ApiVersion(2, 0));
            option.AddDocument(versionname, displayName, $"/openapi/{versionname}.json", isDefault);
        }
    });
}

app.MigrateDb();

app.UseHttpsRedirection();

app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        context.Response.StatusCode =
            StatusCodes.Status500InternalServerError;

        var response = ApiResponse<object>.Error(
            StatusCodes.Status500InternalServerError,
            "An unexpected error occurred while processing the request.");

        await context.Response.WriteAsJsonAsync(response);
    });
});

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();