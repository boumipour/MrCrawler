# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**MrCrawler** is a .NET web crawling framework (C# / .NET 9.0) designed for flexible, extensible HTML parsing and element extraction from web pages. It uses strategy patterns for link discovery and supports XPath-based element mapping for data extraction. The project includes:

- **Crawler.Core**: Reusable web crawling library with pluggable strategies
- **Crawler.Sample.Console**: Console application demonstrating usage (crawls coinmarketcap.com for cryptocurrency prices)

## Architecture & Key Components

### High-Level Design

The framework uses a **strategy pattern** for link discovery and separation of concerns:

1. **CrawlerAgent** (Crawler.Core/CrawlerAgent.cs): Main orchestrator
   - Two crawl modes: CrawlDomainAsync() (recursive domain crawling) and CrawlPagesAsync() (parallel page processing)
   - Accepts an IFindLinkStrategy to discover links and a callback function to process extracted elements
   - Parses HTML using HtmlAgilityPack and evaluates XPath expressions
   - Page processing is fully async and respects throttling via MaxDegreeOfParallelism

2. **CrawlContext** (Crawler.Core/Models/CrawlContext.cs): Configuration container
   - Holds domain URL, XPath patterns, element maps, throttling settings
   - Manages HttpClient with browser-like headers and SSL certificate bypass
   - Provides optional ILogger for diagnostic logging
   - Supports optional deep crawling (MaxDeep), delays, and page URL filtering

3. **Link Discovery Strategies** (Crawler.Core/FindLinkStrategies/)
   - IFindLinkStrategy: Interface for pluggable link discovery
   - BlindlyFindLinkStrategy: Recursive HTML crawling (follows all links respecting MaxDeep and domain boundary)
   - SitemapFindLinkStratesy: Parses standard sitemap.xml files
   - SitemapIndexFindLinkStratesy: Parses sitemap_index.xml for large sites
   - All strategies track visited URIs to avoid duplicates

4. **Link Scheduling** (Crawler.Core/LinkSchedulers/)
   - ILinkScheduler: Interface for link queue management
   - FIFO_QueueLinkScheduler: Uses ConcurrentQueue for thread-safe, ordered processing (currently unused but available for future expansion)

5. **LinkChildFinder** (Crawler.Core/LinkChildFinder.cs): Link extraction helper
   - Fetches a page and extracts all anchor href attributes
   - Validates and converts relative URLs to absolute
   - Filters by domain, scheme (http/https), and depth
   - Used internally by BlindlyFindLinkStrategy

6. **Utility Extensions** (Crawler.Core/Utility/)
   - StringExtensions: Text normalization (Latin number conversion, Persian character mapping, diacritic removal)
   - ExceptionExtensions: Hierarchical exception message aggregation
   - ObjectExtensions, UriExtensions: Null checking, URI path manipulation

### Data Flow

CrawlerAgent processes links through:
1. CrawlContext setup with domain, XPath patterns, element maps
2. IFindLinkStrategy.FindLinksAsync() returns async stream of Link objects
   - LinkChildFinder.FindAsync() used for BlindlyFindLinkStrategy
3. For each discovered Link or input page:
   - Fetch page HTML via HttpClient
   - Parse with HtmlAgilityPack
   - Extract using XPath (PageIdXpath + PageElementMaps dictionary)
   - Pass (context, uri, extracted elements) to callback function

### Design Patterns

- **Strategy Pattern**: IFindLinkStrategy allows swapping link discovery algorithms
- **Async Streams**: IAsyncEnumerable<Link> enables memory-efficient link processing
- **Disposable Pattern**: All major components implement IDisposable
- **Parallel Processing**: CrawlPagesAsync uses Parallel.ForEachAsync with configurable throttling

## Build & Run

### Build
```bash
dotnet build MrCrawler.sln
dotnet build Crawler.Core/Crawler.Core.csproj
dotnet build Crawler.Sample.Console/Crawler.Sample.Console.csproj
```

### Run Console Sample
```bash
dotnet run --project Crawler.Sample.Console/Crawler.Sample.Console.csproj
```

### Watch & Rebuild
```bash
dotnet watch run --project Crawler.Sample.Console/Crawler.Sample.Console.csproj
```

### Publish
```bash
dotnet publish Crawler.Sample.Console/Crawler.Sample.Console.csproj
```

### VSCode Tasks
Pre-configured VSCode tasks available in .vscode/tasks.json:
- **build**: Builds Crawler.Sample.Console
- **publish**: Publishes Crawler.Sample.Console
- **watch**: Runs watch mode for development

### Debug
- Use VSCode debug configuration in .vscode/launch.json (.NET Core Launch)
- Pre-launch task automatically runs build

## Usage Example

From Crawler.Sample.Console/Program.cs:

```csharp
// Create a crawler agent with context
CrawlerAgent crawlerAgent = new(new CrawlContext(
    new Uri("https://coinmarketcap.com"))
{
    Id = Guid.NewGuid().ToString(),
    PageIdXpath = "//*[@id='section-coin-overview']/div[1]/h1/span",
    PageElementMaps = new Dictionary<string, string[]>()
    {
        { "Price", ["//*[@id='section-coin-overview']/div[2]/span"] }
    },
});

// Crawl specific pages
await crawlerAgent.CrawlPagesAsync(
    [new Uri("https://coinmarketcap.com/currencies/bitcoin/")],
    async (context, uri, elements) =>
    {
        var price = elements["Price"].Normalize().Replace("$", "").Replace(",", "");
        if (decimal.TryParse(price, out var value))
            Console.WriteLine(value);
    }
);
```

## Important Implementation Notes

### Known TODOs/Gaps (from code comments)
1. **Parallelization**: CrawlDomainAsync is not parallelized (comment: "todo: use parallel")
2. **String Formatting**: Flag-based approach to per-character string operations in StringExtensions is inefficient
3. **Sitemap Parsing**: Large heap allocations when parsing huge sitemaps (noted as "big heap object problem")
4. **Element Child Node Handling**: Logic to handle child nodes may need refinement

### Critical Behaviors
- **SSL Certificate Validation**: Disabled by default (ServerCertificateCustomValidationCallback returns true) for testing against sites with self-signed certs
- **XPath Engine**: Uses HtmlAgilityPack's XPath support; relative queries use // from document root
- **Deduplication**: Both BlindlyFindLinkStrategy and FIFO_QueueLinkScheduler track visited URIs independently
- **Throttling**: Controlled via CrawlContext.Throttler (default 1); sets MaxDegreeOfParallelism for parallel operations
- **Delay**: CrawlContext.Delay property exists but is not currently enforced in code

### Dependencies
- **HtmlAgilityPack** (1.11.33): DOM parsing and XPath evaluation
- **Microsoft.Extensions.Http** (6.0.0): HTTP client patterns
- **Microsoft.AspNetCore.Http** (2.2.2): HTTP primitives (limited usage, may be legacy)
- **.NET 9.0 SDK**: Target framework

## Code Organization

```
Crawler.Core/
├── CrawlerAgent.cs              # Main orchestrator
├── LinkChildFinder.cs           # Link extraction from pages
├── Models/
│   ├── CrawlContext.cs          # Configuration & HTTP client
│   └── Link.cs                  # URI + depth struct
├── FindLinkStrategies/
│   ├── IFindLinkStrategy.cs     # Strategy interface
│   ├── BlindlyFindLinkStrategy.cs
│   ├── SitemapFindLinkStratesy.cs
│   └── SitemapIndexFindLinkStratesy.cs
├── LinkSchedulers/
│   ├── ILinkScheduler.cs        # Scheduler interface
│   └── FIFO_QueueLinkScheduler.cs
└── Utility/
    ├── StringExtensions.cs
    ├── ExceptionExtensions.cs
    ├── ObjectExtensions.cs
    ├── UriExtensions.cs
    └── (other extension classes)

Crawler.Sample.Console/
└── Program.cs                   # Demo usage
```

## Extending the Framework

### Adding a New Link Discovery Strategy
1. Implement IFindLinkStrategy with FindLinksAsync(CrawlContext)
2. Return IAsyncEnumerable<Link> with yielded results
3. Pass instance to CrawlerAgent.CrawlDomainAsync(strategy, callback)

### Customizing Element Extraction
- Set PageElementMaps dictionary with element keys and XPath value arrays
- XPaths are evaluated in order; first non-empty result is used
- Results stored in callback's Dictionary<string, string>

### Logging
- Provide ILogger to CrawlContext constructor for diagnostic output
- Logged at CRITICAL (errors) and TRACE (success/perf) levels

## Common Issues & Troubleshooting

- **XPath not matching**: Verify selector works in browser DevTools; HtmlAgilityPack may parse malformed HTML differently
- **Relative URL errors**: LinkChildFinder should handle these automatically; check domain is correctly set
- **SSL errors**: Occurs before SSL bypass if HttpClient initialization fails; ensure DNS resolution works
- **Memory usage**: Check for large sitemaps; consider streaming strategies or chunked processing
