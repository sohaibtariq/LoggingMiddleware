using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Middleware
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate next;
        private ILogger<LoggingMiddleware> logger;

        public LoggingMiddleware(RequestDelegate _next, ILogger<LoggingMiddleware> _logger)
        {
            next = _next;
            logger = _logger;
    
        }

        public async Task InvokeAsync(HttpContext context) 
        {
            logger.LogInformation($"Request : {context.Request.Path}");
            
            using (var tempBody = new MemoryStream())
            {
                //store a reference to Http response stream and assign a memory stream to it
                var originalBody = context.Response.Body;
                context.Response.Body = tempBody;
                
                await next(context);
                
                //read the response body from the temp memory stream 
                context.Response.Body.Seek(0, SeekOrigin.Begin);
                var response = await new StreamReader(context.Response.Body).ReadToEndAsync();
                //copy the memory stream to the Http response stream                
                context.Response.Body.Seek(0, SeekOrigin.Begin);
                await tempBody.CopyToAsync(originalBody);
                logger.LogInformation($"Response : {response}");
            }
        }
    }
}
