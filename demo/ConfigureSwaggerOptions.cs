using System.Linq;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi.Any;

namespace demo
{
    public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        readonly IApiVersionDescriptionProvider provider;
        public ConfigureSwaggerOptions( IApiVersionDescriptionProvider provider ) => this.provider = provider;

        public void Configure( SwaggerGenOptions options )
        {
            foreach ( var description in provider.ApiVersionDescriptions )
            {
                options.SwaggerDoc( description.GroupName, new OpenApiInfo());
                options.OperationFilter<ApiVersionFilter>();
            }
        }


        internal class ApiVersionFilter : IOperationFilter
        {
            public void Apply(OpenApiOperation operation, OperationFilterContext context)
            {
                var apiDescription = context.ApiDescription;

                operation.Deprecated |= apiDescription.IsDeprecated();

                if ( operation.Parameters == null )
                {
                    return;
                }

                foreach ( var parameter in operation.Parameters )
                {
                    var description = apiDescription.ParameterDescriptions.First( p => p.Name == parameter.Name );

                    if ( parameter.Description == null )
                    {
                        parameter.Description = description.ModelMetadata?.Description;
                    }

                    if ( parameter.Schema.Default == null && description.DefaultValue != null )
                    {
                        parameter.Schema.Default = new OpenApiString( description.DefaultValue.ToString() );
                    }

                    parameter.Required |= description.IsRequired;
                }
            }
        }
    
    }
    
}