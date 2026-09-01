using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using ResortBooking.API.Data;
using ResortBooking.API.Dtos;
using ResortBooking.API.Models;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var key = Encoding.ASCII.GetBytes(builder.Configuration.GetSection("JwtSettings")["Secret"]);

// Add services to the container.

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
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

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
    o.CreateMap<Villa, UpdateVillaDto>().ReverseMap();
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

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();