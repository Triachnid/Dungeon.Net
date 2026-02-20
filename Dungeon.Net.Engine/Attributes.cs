using System;

namespace Dungeon.Net.Engine;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class PhaseComponentAttribute(int phase, string layoutSection) : Attribute
{
    public int Phase { get; } = phase;
    public string LayoutSection { get; } = layoutSection;
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class PhaseAttribute(int sequence) : Attribute
{
    public int Sequence { get; } = sequence;
}
