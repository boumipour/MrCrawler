using System;

namespace Crawler.Core
{
    public interface ILinkScheduler : IDisposable
    {
        int Count { get; }

        bool Add(Link link);
        Link Get();
    }
}
