using System;
using System.Collections.Generic;
using System.Linq;

namespace Crawler.Core.FindLinkSteratesies
{
    public class BlindlyFindLinkStrategy : IFindLinkStrategy
    {
        private readonly Dictionary<string, bool> _scheduledUris = new();
        private bool disposedValue;

        public async IAsyncEnumerable<Link> FindLinksAsync(CrawlContext context)
        {
            await foreach (var link in FindLinksFromLinkAsync(new Link(context.Domain, 0), context))
            {
                if (context.PageUrlFilters.Length > 0)
                {
                    if (!context.PageUrlFilters.Any(filter => link.Uri.AbsoluteUri.Contains(filter)))
                    {
                        continue;
                    }
                }

                yield return link;
            }
        }

        private async IAsyncEnumerable<Link> FindLinksFromLinkAsync(Link link, CrawlContext context)
        {
            using LinkChildFinder linkfinder = new(link, context);

            await foreach (Link detectedLink in linkfinder.FindAsync())
            {
                string hashCode = detectedLink.GetHashCode().ToString();
                bool linkAdded = _scheduledUris.TryAdd(hashCode, false);
                if (!linkAdded)
                {
                    continue;
                }

                yield return detectedLink;

                if (detectedLink.Deep <= context.MaxDeep)
                {
                    await foreach (var child in FindLinksFromLinkAsync(detectedLink, context))
                    {
                        yield return child;
                    }
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _scheduledUris?.Clear();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
