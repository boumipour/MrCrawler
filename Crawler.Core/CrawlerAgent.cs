using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Utility.Extensions;

namespace Crawler.Core
{
    public class CrawlerAgent : IDisposable
    {
        private readonly CrawlContext _context;
        private bool disposedValue;

        public CrawlerAgent(CrawlContext context)
        {
            _context = context;
        }

        // todo: use parallel
        public async Task CrawlDomainAsync(IFindLinkStrategy findLinkStrategy, Func<CrawlContext, Uri, Dictionary<string, string>, Task> pageProcessorAsync)
        {
            await foreach (var pageLink in findLinkStrategy.FindLinksAsync(_context))
            {
                await ParsPageAsync(nameof(CrawlDomainAsync), pageLink.Uri, _context.PageIdXpath, pageProcessorAsync);
            }
        }

        public Task CrawlPagesAsync(List<Uri> pages, Func<CrawlContext, Uri, Dictionary<string, string>, Task> pageProcessorAsync)
        {
            ParallelOptions parallelOptions = new()
            {
                MaxDegreeOfParallelism = _context.Throttler
            };

            return Parallel.ForEachAsync(pages, parallelOptions, async (page, cancellationToken) =>
            {
                await ParsPageAsync(nameof(CrawlPagesAsync), page, _context.PageIdXpath, pageProcessorAsync);
            });
        }

        private async Task ParsPageAsync(string serviceName, Uri pageUri, string pageIdXpath, Func<CrawlContext, Uri, Dictionary<string, string>, Task> pageProcessorAsync)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            Dictionary<string, string> foundElements = new();
            try
            {
                if (string.IsNullOrEmpty(pageIdXpath))
                {
                    return;
                }

                Stream pageStream = await _context.HttpClient.GetStreamAsync(pageUri);

                HtmlDocument htmlDocument = new();
                htmlDocument.Load(pageStream, Encoding.UTF8, false);

                string pageId = htmlDocument.DocumentNode?
                                            .SelectNodes(pageIdXpath)?
                                            .FirstOrDefault()?.InnerText
                                            ?.Trim() ?? "";

                if (string.IsNullOrEmpty(pageId))
                {
                    return;
                }

                foreach (var elementMaps in _context.PageElementMaps)
                {
                    string value = string.Empty;

                    for (int i = 0; i < elementMaps.Value.Length; i++)
                    {
                        var node = htmlDocument.DocumentNode?.SelectNodes(elementMaps.Value[i])?.FirstOrDefault();

                        // todo: check for every elemnt/ is it currect ?
                        if (node?.HasChildNodes ?? false)
                        {
                            node = node.FirstChild;
                        }

                        value = node?.InnerText?.FixStringFormat() ?? "";

                        if (!string.IsNullOrEmpty(value))
                        {
                            break;
                        }
                    }

                    foundElements.Add(elementMaps.Key, value);
                }

                if (foundElements.All(foundElement => string.IsNullOrEmpty(foundElement.Value)))
                {
                    return;
                }

                await pageProcessorAsync(_context, pageUri, foundElements);
            }
            catch (Exception exception)
            {
                _context.Logger.LogCritical("{@message}", new
                {
                    ServiceName = serviceName,
                    ActionName = nameof(ParsPageAsync),
                    Domain = _context.Domain.AbsoluteUri,
                    Page = pageUri,
                    Elapsed = stopWatch.ElapsedMilliseconds,
                    Message = exception.GetJoinedMessageFromHierarchy(ex => ex.InnerException),
                    ExceptionType = exception.GetType(),
                    Exception = exception
                });
            }
            finally
            {
                _context.Logger.LogTrace("{@message}", new
                {
                    ServiceName = serviceName,
                    ActionName = nameof(ParsPageAsync),
                    Domain = _context.Domain.AbsoluteUri,
                    Page = pageUri,
                    Elapsed = stopWatch.ElapsedMilliseconds,
                    Message = $"page parsed {string.Join("|", foundElements.Where(w => !string.IsNullOrEmpty(w.Value)).ToList())}",
                });
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _context?.Dispose();
                }

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
