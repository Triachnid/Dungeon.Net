using Dungeon.Net.Engine;
using Spectre.Console;

namespace Dungeon.Net.MonsterCrawl;

[Phase(2)]
public class CrawlLayoutFactory : ILayoutFactory
{
    public Layout CreateLayout()
        => new Layout("Root")
            .SplitRows(
                new Layout("HP").Size(1),
                new Layout("Room").Size(10),
                new Layout("Weapon").Size(10)
            );
}

[Phase(3)]
public class ScoreLayoutFactory : ILayoutFactory
{
    public Layout CreateLayout()
        => new Layout("Root");
}