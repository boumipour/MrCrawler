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
        public string PageIdXpath { get; set; }
        public Dictionary<string, string[]> PageElementMaps { get; set; }
        public Dictionary<string, object> AdditionalData { get; set; }

        public int MaxDeep { get; set; }
        public int Throttler { get; set; } = 1;
        public int Delay { get; set; } = 0;

        public ILinkScheduler Scheduler { get; }
        public HttpClient HttpClient { get; }
        public ILogger Logger { get; }

        public CrawlContext(Uri domain, ILogger logger, int httpConnectionLimit = 0)
        {
            Domain = domain;
            Logger = logger;

            HttpClientHandler clientHandler = new()
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; }
            };
            HttpClient = new HttpClient(clientHandler);

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
