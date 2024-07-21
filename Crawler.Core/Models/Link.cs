using System;

namespace Crawler.Core
{
    public struct Link
    {
        public Uri Uri { get; }
        public int Deep { get; }

        public Link(Uri uri, int depp)
        {
            Uri = uri;
            Deep = depp;
        }

        public override int GetHashCode()
        {
            return Uri.GetHashCode();
        }

        public override string ToString()
        {
            return Uri.ToString();
        }

        public bool IsEmpty()
        {
            return Uri == null;
        }
    }
}
