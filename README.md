# MrCrawler

[![NuGet](https://img.shields.io/nuget/v/MrCrawler.svg)](https://www.nuget.org/packages/MrCrawler/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/MrCrawler.svg)](https://www.nuget.org/packages/MrCrawler/)
[![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%209.0-512BD4)](https://dotnet.microsoft.com)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

A flexible, async .NET web crawling framework with **XPath-based element extraction** and a **pluggable strategy pattern** for link discovery.

---

## Features

- **XPath element extraction** — declare what to extract via a dictionary of XPath expressions; no HTML parsing boilerplate needed
- **Pluggable link discovery** — swap between blind recursive crawling, `sitemap.xml`, or `sitemap_index.xml` with a single line
- **Async streams** — `IAsyncEnumerable<Link>` throughout; pages are processed as they are discovered, not buffered
- **Parallel processing** — configurable throttling via `Parallel.ForEachAsync`
- **Browser-like HTTP headers** — realistic `User-Agent`, `Accept`, and `Referer` headers out of the box
- **Optional `ILogger` support** — plugs into Microsoft.Extensions.Logging

---

## Installation

```bash
dotnet add package MrCrawler
```

---

## Quick Start

### Scrape a list of known pages

```csharp
using Crawler.Core;

var context = new CrawlContext(new Uri("https://coinmarketcap.com"))
{
    PageIdXpath = "//*[@id='section-coin-overview']/div[1]/h1/span",
    PageElementMaps = new Dictionary<string, string[]>
    {
        { "Price",  ["//*[@id='section-coin-overview']/div[2]/span"] },
        { "Volume", ["//*[@id='section-coin-overview']/div[3]/span"] },
    },
    Throttler = 4,  // max 4 parallel requests
};

using CrawlerAgent agent = new(context);

await agent.CrawlPagesAsync(
    [
        new Uri("https://coinmarketcap.com/currencies/bitcoin/"),
        new Uri("https://coinmarketcap.com/currencies/ethereum/"),
    ],
    async (ctx, uri, elements) =>
    {
        Console.WriteLine($"{uri.Host}: Price = {elements["Price"]}");
    }
);
```

### Crawl an entire domain recursively

```csharp
using Crawler.Core;
using Crawler.Core.FindLinkStrategies;

var context = new CrawlContext(new Uri("https://example.com"))
{
    MaxDeep        = 3,             // follow links up to 3 levels deep
    PageUrlFilters = ["/blog/"],    // only yield URLs that contain /blog/
    PageElementMaps = new Dictionary<string, string[]>
    {
        { "Title", ["//h1", "//title"] },  // try //h1, fall back to //title
    },
};

using CrawlerAgent agent = new(context);
using BlindlyFindLinkStrategy strategy = new();

await agent.CrawlDomainAsync(strategy, async (ctx, uri, elements) =>
{
    Console.WriteLine($"{uri}: {elements["Title"]}");
});
```

### Crawl via sitemap.xml

```csharp
using Crawler.Core;
using Crawler.Core.FindLinkStrategies;

var context = new CrawlContext(new Uri("https://example.com"))
{
    SitemapPath = "/sitemap.xml",
    PageElementMaps = new Dictionary<string, string[]>
    {
        { "Title", ["//h1"] },
    },
};

using CrawlerAgent agent = new(context);
using SitemapFindLinkStrategy strategy = new();

await agent.CrawlDomainAsync(strategy, async (ctx, uri, elements) =>
{
    Console.WriteLine($"{uri}: {elements["Title"]}");
});
```

### Crawl via sitemap index (large sites with multiple sitemaps)

```csharp
using SitemapIndexFindLinkStrategy strategy = new();
// Same pattern as above — handles sites that use a sitemap_index.xml
// pointing to multiple child sitemaps.
```

---

## API Reference

### `CrawlContext`

| Property | Type | Default | Description |
|---|---|---|---|
| `Domain` | `Uri` | *(constructor)* | Base domain for the crawl |
| `Id` | `string` | `null` | Optional identifier, appears in log output |
| `PageIdXpath` | `string` | `""` | XPath to verify relevance — page is skipped if nothing matches |
| `PageElementMaps` | `Dictionary<string, string[]>` | required | Key → ordered XPath fallbacks; first non-empty result wins |
| `MaxDeep` | `int` | `0` | Max link depth for recursive crawling |
| `Throttler` | `int` | `1` | Max concurrent HTTP requests in `CrawlPagesAsync` |
| `Delay` | `int` | `0` | Delay in ms between requests *(reserved, not yet enforced)* |
| `SitemapPath` | `string` | `null` | Relative path to sitemap (e.g. `/sitemap.xml`) |
| `PageUrlFilters` | `string[]` | `[]` | Only yield URLs containing at least one of these substrings |
| `AdditionalData` | `Dictionary<string, object>` | `null` | Pass custom data through to your callback |

**Constructor:** `CrawlContext(Uri domain, ILogger? logger = null, int httpConnectionLimit = 0)`

### `CrawlerAgent`

| Method | Description |
|---|---|
| `CrawlDomainAsync(IFindLinkStrategy, callback)` | Discover links via a strategy, then extract elements from each page |
| `CrawlPagesAsync(List<Uri>, callback)` | Extract elements from a known list of pages in parallel |

**Callback signature:** `Func<CrawlContext, Uri, Dictionary<string, string>, Task>`

### Link Discovery Strategies

| Strategy | Namespace | Description |
|---|---|---|
| `BlindlyFindLinkStrategy` | `Crawler.Core.FindLinkStrategies` | Recursively follows all `<a href>` links within the domain up to `MaxDeep` |
| `SitemapFindLinkStrategy` | `Crawler.Core.FindLinkStrategies` | Parses a standard `sitemap.xml` (`<urlset>`) |
| `SitemapIndexFindLinkStrategy` | `Crawler.Core.FindLinkStrategies` | Parses a `sitemap_index.xml`, then fetches and parses each child sitemap |

All strategies implement `IFindLinkStrategy` — you can provide your own implementation.

---

## Architecture

```
CrawlerAgent
├── CrawlDomainAsync(IFindLinkStrategy, callback)   ← strategy-driven discovery
│   └── IFindLinkStrategy.FindLinksAsync()
│       ├── BlindlyFindLinkStrategy  → LinkChildFinder (parses <a href>)
│       ├── SitemapFindLinkStrategy  → parses sitemap.xml <urlset>
│       └── SitemapIndexFindLinkStrategy → parses sitemap_index.xml,
│                                          then each child sitemap
└── CrawlPagesAsync(List<Uri>, callback)             ← direct page list
    └── Parallel.ForEachAsync (throttled by Throttler)
        └── HtmlAgilityPack XPath extraction → callback
```

---

## Requirements

- .NET 8.0 or .NET 9.0
- [HtmlAgilityPack](https://www.nuget.org/packages/HtmlAgilityPack/)
- [Microsoft.Extensions.Http](https://www.nuget.org/packages/Microsoft.Extensions.Http/)

---

## License

[MIT](LICENSE) — Copyright © 2024-2026 Majid Boumipour
