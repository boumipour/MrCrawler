using System;
using System.Collections.Generic;
using System.Diagnostics;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Utility.Extensions;

namespace Crawler.Core
{
    public class LinkChildFinder : IDisposable
    {
        private readonly Link _link;
        private readonly CrawlContext _context;

        private bool disposedValue;
        private string pagehtml = string.Empty;
        private HtmlDocument htmlDocument = null;
        private HtmlNodeCollection hrefs = null;

        public LinkChildFinder(Link link, CrawlContext context)
        {
            _link = link;
            _context = context;
        }

        public async IAsyncEnumerable<Link> FindAsync()
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            int deep = _link.Deep + 1;

            if (deep > _context.MaxDeep)
            {
                yield break;
            }

            try
            {
                pagehtml = await _context.HttpClient.GetStringAsync(_link.Uri);
                if (string.IsNullOrEmpty(pagehtml))
                {
                    yield break;
                }

                htmlDocument = new();
                htmlDocument.LoadHtml(pagehtml);

                hrefs = htmlDocument.DocumentNode?.SelectNodes("//a[@href]");
            }
            catch (Exception exception)
            {
                _context.Logger.LogCritical("{@message}", new
                {
                    ServiceName = nameof(LinkChildFinder),
                    ActionName = nameof(FindAsync),
                    Domain = _context.Domain.AbsoluteUri,
                    Page = _link.Uri.AbsoluteUri,
                    Elapsed = stopWatch.ElapsedMilliseconds,
                    Message = exception.GetJoinedMessageFromHierarchy(ex => ex.InnerException),
                    ExceptionType = exception.GetType(),
                    Exception = exception
                });

                yield break;
            }

            if (hrefs.IsNull())
            {
                yield break;
            }

            foreach (HtmlNode href in hrefs)
            {
                stopWatch.Restart();
                Uri finalUri;

                try
                {
                    string hrefValue = href.GetAttributeValue("href", string.Empty);

                    if (string.IsNullOrEmpty(hrefValue))
                    {
                        continue;
                    }

                    // pars hrefValue to uri
                    Uri.TryCreate(hrefValue, UriKind.Absolute, out Uri uri);
                    if (uri.IsNull())
                    {
                        continue;
                    }

                    // for relative path
                    if (string.IsNullOrEmpty(uri.Host))
                    {
                        Uri.TryCreate($"{_context.Domain.Scheme}://{_context.Domain.Host}{uri.AbsolutePath}", UriKind.Absolute, out uri);
                        if (uri.IsNull())
                        {
                            continue;
                        }
                    }

                    if (uri.AbsoluteUri == _link.Uri.AbsoluteUri)
                    {
                        continue;
                    }

                    if (uri.Host != _context.Domain.Host)
                    {
                        continue;
                    }

                    if (uri.Scheme != "http" && uri.Scheme != "https")
                    {
                        continue;
                    }

                    finalUri = uri;
                }
                catch (Exception exception)
                {
                    _context.Logger.LogCritical("{@message}", new
                    {
                        ServiceName = nameof(LinkChildFinder),
                        ActionName = nameof(FindAsync),
                        Domain = _context.Domain.AbsoluteUri,
                        Page = _link.Uri.AbsoluteUri,
                        Elapsed = stopWatch.ElapsedMilliseconds,
                        Message = exception.GetJoinedMessageFromHierarchy(ex => ex.InnerException),
                        ExceptionType = exception.GetType(),
                        Exception = exception
                    });

                    continue;
                }

                yield return new(finalUri, deep);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                pagehtml = null;
                htmlDocument = null;
                hrefs = null;

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
