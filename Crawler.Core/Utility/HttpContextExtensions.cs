using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Utility.Extensions
{
    public static class HttpContextExtensions
    {
        public static async Task<object> GetInputParamsAsync(this HttpContext httpContext)
        {
            try
            {
                // IMPORTANT: Ensure the requestBody can be read multiple times.
                httpContext.Request.EnableBuffering();

                var reader = new StreamReader(httpContext.Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
                string body = await reader.ReadToEndAsync();

                httpContext.Request.Body.Position = 0;

                return new
                {
                    queryString = httpContext.Request.QueryString.Value,
                    body
                };
            }
            catch (Exception)
            {
                // ignored
                return null;
            }
        }
    }
}
