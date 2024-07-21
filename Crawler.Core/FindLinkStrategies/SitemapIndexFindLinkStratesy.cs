using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using Microsoft.Extensions.Logging;
using Utility.Extensions;

namespace Crawler.Core.FindLinkStrategies
{
    public class SitemapIndexFindLinkStratesy : IFindLinkStrategy
    {
        private bool disposedValue;

        public async IAsyncEnumerable<Link> FindLinksAsync(CrawlContext context)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            XmlNodeList xmlSiteMapList;

            try
            {
                // todo: big heap object problem.
                string siteMapIndexContent = await context.HttpClient.GetStringAsync(context.Domain + context.SitemapPath);

                XmlDocument siteMapIndexDoc = new();
                siteMapIndexDoc.LoadXml(siteMapIndexContent);

                xmlSiteMapList = siteMapIndexDoc.GetElementsByTagName("sitemap");
                if (xmlSiteMapList.IsNull())
                {
                    yield break;
                }
            }
            catch (Exception exception)
            {
                context.Logger.LogCritical("{@message}", new
                {
                    ServiceName = nameof(SitemapIndexFindLinkStratesy),
                    ActionName = nameof(FindLinksAsync),
                    Domain = context.Domain.AbsoluteUri,
                    Page = context.SitemapPath,
                    Elapsed = stopWatch.ElapsedMilliseconds,
                    Message = exception.GetJoinedMessageFromHierarchy(ex => ex.InnerException),
                    ExceptionType = exception.GetType(),
                    Exception = exception
                });

                yield break;
            }

            foreach (XmlNode xmlSiteMap in xmlSiteMapList)
            {
                XmlNodeList xmlPageLinkList;
                stopWatch.Restart();

                string siteMapUri = string.Empty;
                try
                {
                    siteMapUri = xmlSiteMap["loc"]?.InnerText?.Trim() ?? string.Empty;
                    if (string.IsNullOrEmpty(siteMapUri))
                    {
                        continue;
                    }

                    string siteMapContent = await context.HttpClient.GetStringAsync(siteMapUri);

                    XmlDocument siteMapDoc = new();
                    siteMapDoc.LoadXml(siteMapContent);

                    xmlPageLinkList = siteMapDoc.GetElementsByTagName("url");
                }
                catch (Exception exception)
                {
                    context.Logger.LogCritical("{@message}", new
                    {
                        ServiceName = nameof(SitemapIndexFindLinkStratesy),
                        ActionName = nameof(FindLinksAsync),
                        Domain = context.Domain.AbsoluteUri,
                        Page = siteMapUri,
                        Elapsed = stopWatch.ElapsedMilliseconds,
                        Message = exception.GetJoinedMessageFromHierarchy(ex => ex.InnerException),
                        ExceptionType = exception.GetType(),
                        Exception = exception
                    });

                    continue;
                }

                foreach (XmlNode xmlPageLink in xmlPageLinkList.Cast<XmlNode>())
                {
                    stopWatch.Restart();
                    string pagelink = string.Empty;

                    try
                    {
                        pagelink = xmlPageLink["loc"]?.InnerText?.Trim() ?? string.Empty;
                        if (string.IsNullOrEmpty(pagelink))
                        {
                            continue;
                        }

                        if (context.PageUrlFilters.Length > 0)
                        {
                            if (!context.PageUrlFilters.Any(filter => pagelink.Contains(filter)))
                            {
                                continue;
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        context.Logger.LogCritical("{@message}", new
                        {
                            ServiceName = nameof(SitemapIndexFindLinkStratesy),
                            ActionName = nameof(FindLinksAsync),
                            Domain = context.Domain.AbsoluteUri,
                            Page = pagelink,
                            Elapsed = stopWatch.ElapsedMilliseconds,
                            Message = exception.GetJoinedMessageFromHierarchy(ex => ex.InnerException),
                            ExceptionType = exception.GetType(),
                            Exception = exception
                        });

                        continue;
                    }

                    yield return new Link(new Uri(pagelink), 0);
                }
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
