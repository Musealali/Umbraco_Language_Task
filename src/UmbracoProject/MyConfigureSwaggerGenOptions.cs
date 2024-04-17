using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace UmbracoProject;

public class MyConfigureSwaggerGenOptions : IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        options.SwaggerDoc(
            "my-api-v1",
            new OpenApiInfo
            {
                Title = "My API v1",
                Version = "1.0",
            });
    }
}

public static class MyConfigureSwaggerGenUmbracoBuilderExtensions
{
    public static IUmbracoBuilder ConfigureMySwaggerGen(this IUmbracoBuilder builder)
    {
        // call this from Program.cs, i.e.:
        //     builder.CreateUmbracoBuilder()
        //         ...
        //         .ConfigureMySwaggerGen()
        //         .Build();
        builder.Services.ConfigureOptions<MyConfigureSwaggerGenOptions>();
        return builder;
    }
}