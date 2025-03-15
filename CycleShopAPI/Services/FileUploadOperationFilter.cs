using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Http;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Linq;
using System;
using System.Collections.Generic;

namespace CycleShopAPI.Services
{
    public class FileUploadOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Check if any of the parameters are IFormFile type
            var formFileParams = context.MethodInfo.GetParameters()
                .Where(p => p.ParameterType == typeof(IFormFile) || p.ParameterType == typeof(IFormFileCollection));

            if (formFileParams.Any())
            {
                // Set up a multipart/form-data content type
                var mediaType = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = new Dictionary<string, OpenApiSchema>(),
                        Required = new HashSet<string>()
                    }
                };

                // Add each IFormFile parameter
                foreach (var param in formFileParams)
                {
                    mediaType.Schema.Properties.Add(param.Name, new OpenApiSchema
                    {
                        Type = "string",
                        Format = "binary"
                    });
                }

                // Replace or add multipart/form-data content type
                operation.RequestBody = new OpenApiRequestBody
                {
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["multipart/form-data"] = mediaType
                    },
                    Required = true
                };

                // Remove the parameter from the parameter list since it's now in the request body
                var fileParamNames = formFileParams.Select(p => p.Name).ToList();
                var parametersToRemove = operation.Parameters
                    .Where(p => fileParamNames.Contains(p.Name))
                    .ToList();

                foreach (var param in parametersToRemove)
                {
                    operation.Parameters.Remove(param);
                }
            }
        }
    }
}