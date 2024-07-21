using System;
using System.Collections.Generic;

namespace Crawler.Core
{
    public interface IFindLinkStrategy : IDisposable
    {
        IAsyncEnumerable<Link> FindLinksAsync(CrawlContext context);
    }
}
