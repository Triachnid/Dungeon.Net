using System.Threading.Tasks;
using Spectre.Console;

namespace Dungeon.Net.MonsterCrawl;

public static class MonsterCrawlLauncher
{
    public static async Task LaunchAsync()
    {
        GameLoader.LoadGame();
        await GameLoader.StartLoadedGameAsync(CrawlContext.GameEndTokenSource.Token);
    }
}