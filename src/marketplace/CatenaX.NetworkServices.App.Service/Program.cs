using CatenaX.NetworkServices.App.Service.BusinessLogic;
using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var VERSION = "v2";
var TAG = typeof(Program).Namespace;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSwaggerGen(c => c.SwaggerDoc(VERSION, new OpenApiInfo { Title = TAG, Version = VERSION }));
builder.Services.AddDbContext<PortalDBContext>(o => o.UseNpgsql(builder.Configuration.GetConnectionString("PortalDb")));
builder.Services.AddTransient<IAppsBusinessLogic, AppsBusinessLogic>();

var app = builder.Build();

// Configure the HTTP request pipeline.

if (app.Configuration.GetValue<bool?>("SwaggerEnabled") != null && app.Configuration.GetValue<bool>("SwaggerEnabled"))
{
    app.UseSwagger(c => c.RouteTemplate = "/api/apps/swagger/{documentName}/swagger.{json|yaml}");
    app.UseSwaggerUI(c => {
        c.SwaggerEndpoint(string.Format("/api/apps/swagger/{0}/swagger.json", VERSION), string.Format("{0} {1}", TAG, VERSION));
        c.RoutePrefix = "api/apps/swagger";
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
