using Crawler.Core;


Console.WriteLine("Hello, This is a small sample to using of MrCrawler!");



CrawlerAgent crawlerAgent = new(new(new("https://coinmarketcap.com"))
{
    Id = Guid.NewGuid().ToString(),
    PageIdXpath= "//*[@id=\"section-coin-overview\"]/div[1]/h1/span",
    PageElementMaps = new Dictionary<string, string[]>()
                    {
                      {"Price", ["//*[@id=\"section-coin-overview\"]/div[2]/span"] }
                    },
});


await crawlerAgent.CrawlPagesAsync([new("https://coinmarketcap.com/currencies/bitcoin/")], (crawlContext, uri, elements) =>
{
    if (elements == null || !elements.Any())
        return Task.CompletedTask;

    var priceElement = elements["Price"].Normalize().Replace("$", "").Replace(",", "");

    if (decimal.TryParse(priceElement, out var price))
    {
        Console.WriteLine(price);
    }

    return Task.CompletedTask;
});


Console.WriteLine("Done");
Console.ReadLine();

