using System;

namespace Utility.Extensions
{
    public static class UriExtensions
    {
        public static Uri ClearPath(this Uri uri)
        {
            try
            {
                var schema = uri.Scheme;
                var host = uri.Host;
                var path = uri.LocalPath ?? "";

                return new Uri($"{schema}://{host}{path}");
            }
            catch
            {
                return null;
            }
        }
    }
}
