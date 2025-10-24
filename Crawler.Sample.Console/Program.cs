using Crawler.Core;

Console.WriteLine("Hello, This is a small sample for using of MrCrawler!");


using CrawlerAgent crawler = new (new CrawlContext(new Uri("http://google.com")));

List<Uri> pages = new List<Uri>()
{
    new Uri("http://google.com"),
};

await crawler.CrawlPagesAsync(pages, async (ctx, data, x) => 
{
    
});

Console.WriteLine("Done");

