using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

using Crawler.Core.Schedulers;

using Microsoft.Extensions.Logging;

namespace Crawler.Core
{
    public class CrawlContext : IDisposable
    {
        private bool disposedValue;

        public string Id { get; set; }
        public Uri Domain { get; }
        public string SitemapPath { get; set; }
        public string[] PageUrlFilters { get; set; }
        public string PageIdXpath { get; set; } = string.Empty;
        public Dictionary<string, string[]> PageElementMaps { get; set; }
        public Dictionary<string, object> AdditionalData { get; set; }

        public int MaxDeep { get; set; }
        public int Throttler { get; set; } = 1;
        public int Delay { get; set; } = 0;

        public ILinkScheduler Scheduler { get; }
        public HttpClient HttpClient { get; }
        public ILogger? Logger { get; } = null;

        public CrawlContext(Uri domain, ILogger? logger = null, int httpConnectionLimit = 0)
        {
            Domain = domain;
            Logger = logger;

            HttpClientHandler clientHandler = new()
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; }
            };
            
            HttpClient = new HttpClient(clientHandler);
            
            //add these headers to show normal client
            HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:126.0) Gecko/20100101 Firefox/126.0");
            HttpClient.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            HttpClient.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-US,en;q=0.5");
            HttpClient.DefaultRequestHeaders.Referrer = new Uri("https://www.google.com/");

            if (httpConnectionLimit > 0)
            {
                ServicePointManager.DefaultConnectionLimit = httpConnectionLimit;
            }

            Scheduler = new FIFO_QueueLinkScheduler();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Scheduler?.Dispose();
                    // HttpClient?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
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
