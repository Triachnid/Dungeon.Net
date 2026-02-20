using System.Threading;

namespace Dungeon.Net.MonsterCrawl;

// This should probably be baked into state and injected to components somehow
public static class CrawlContext
{
    public static CancellationTokenSource GameEndTokenSource { get; set; }
    public static CrawlState State { get; set; }

    static CrawlContext() 
    {
        GameEndTokenSource = new();
        State = new();
    }
}